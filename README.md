# revoltNotify
A simple custom notification solution for Revolt.

### Background

revoltNotify was originally written as a way to receive Revolt notifications on iOS, as no native app currently exists for the platform. However, it can also be utilized on Android (and potentially other platforms). It utilizes the [Alertzy](https://alertzy.app/) API by default, but it could easily be hooked up to any other provider.

By default, it currently polls for unread messages every minute. This can probably be raised, but please note that you may run into rate limit issues by doing this, especially if you are using Revolt while the service is running.

It currently does not support receiving notifications from servers.
