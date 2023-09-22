using System.Net.Http.Json;
using System.Text.Json;

using HttpClient client = new();
client.DefaultRequestHeaders.Accept.Clear();

await ProcessLoginRequestAsync(client);

static async Task ProcessLoginRequestAsync(HttpClient client)
{
	var appDir = AppContext.BaseDirectory;
	SessionInfo sessionInfo = new();
	
	LoginRequest loginReq = new();

	if (File.Exists($"{appDir}session_info.json"))
	{
		var jsonString = File.ReadAllText($"{appDir}session_info.json");
		sessionInfo = JsonSerializer.Deserialize<SessionInfo>(jsonString);
	}
	else
	{
		Console.WriteLine(
			"Please enter your Revolt login information. You will only need to do this once.\nThese credentials are " +
			"only sent to Revolt for authentication and are not stored.\n");
		Console.Write("Enter your email: ");
		loginReq.email = Console.ReadLine();
		Console.Write("\nEnter your password: ");
		loginReq.password = Console.ReadLine();
		loginReq.friendly_name = "revoltNotify Service";
		
		Console.Write("\nPlease enter your Alertzy account key: ");
		sessionInfo.alertzyKey = Console.ReadLine();

		var response = await client.PostAsJsonAsync(
			"https://api.revolt.chat/auth/session/login", loginReq);

		var loginRes = await response.Content.ReadAsAsync<LoginResponse>();

		if (!loginRes.result.Equals("Success"))
		{
			Console.WriteLine("\nFailed to login to Revolt. Check to make sure you've entered the correct credentials.");
			return;
		}
		else Console.WriteLine("\nSuccessfully logged in.");
		
		sessionInfo.token = loginRes.token;
		sessionInfo.user_id = loginRes.user_id;

		var json = JsonSerializer.Serialize(sessionInfo);
		File.WriteAllText($"{appDir}session_info.json", json);
	}
	
	client.DefaultRequestHeaders.Add("x-session-token", sessionInfo.token);

	var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

	while (await timer.WaitForNextTickAsync())
	{
		await GetMessagesFromChannelAsync(client, sessionInfo.user_id, sessionInfo.alertzyKey);
	}
}

static async Task GetMessagesFromChannelAsync(HttpClient client, string user_id, string alertzyKey)
{
	var fetchUserRes = await client.GetFromJsonAsync<User>($"https://api.revolt.chat/users/{user_id}");
	
	if (fetchUserRes.status.presence.Equals("Invisible")) 
	{
		var unreadRes = await client.GetFromJsonAsync<UnreadMessage[]>("https://api.revolt.chat/sync/unreads");

		for (var i = 0; i < unreadRes?.Length; i++)
		{
			var fetchNewMsgRes = await client.GetFromJsonAsync<Message[]>(
				$"https://api.revolt.chat/channels/{unreadRes[i]._id.channel}/messages?after={unreadRes[i].last_id}");

			if (fetchNewMsgRes?.Length == 0) continue;
			StringContent stringContent = new("");
			for (var i2 = 0; i2 < fetchNewMsgRes?.Length; i2++)
			{
				var acknowledgeMsgReq = await client.PutAsync(
					$"https://api.revolt.chat/channels/{fetchNewMsgRes[i2].channel}/ack/{fetchNewMsgRes[i2]._id}",
					stringContent);
			}

			var fetchMsgAuthor = await client.GetFromJsonAsync<User>(
				$"https://api.revolt.chat/users/{fetchNewMsgRes[0].author}");

			Dictionary<string, string> alertzyReq = new()
			{
				{ "accountKey", alertzyKey },
				{ "title", "Revolt" },
				{ "message", $"{fetchMsgAuthor.username}: {fetchNewMsgRes[0].content}" }
			};

			using var req = new HttpRequestMessage(HttpMethod.Post, "https://alertzy.app/send")
				{ Content = new FormUrlEncodedContent(alertzyReq) };
			var response = await client.SendAsync(req);

			var alertzyRes = await response.Content.ReadAsAsync<AlertzyResponse>();

			if (alertzyRes.response.Equals("success")) continue;
			Console.WriteLine("Failed to send push notification.");
		}
	}
}
