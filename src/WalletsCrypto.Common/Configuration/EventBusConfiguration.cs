using System;

namespace WalletsCrypto.Common.Configuration
{
    public class EventBusConfiguration
    {
        public string HostName { get; }

        public EventBusConfiguration(string hostName, string username, string password, int retryCount, string subscriptionClientName)
        {
            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentNullException(nameof(hostName));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(subscriptionClientName))
                throw new ArgumentNullException(nameof(subscriptionClientName));


            HostName = hostName;
            Username = username;
            Password = password;
            RetryCount = retryCount;
            SubscriptionClientName = subscriptionClientName;
        }

        public string Username { get; }
        public string Password { get; }
        public int RetryCount { get;  }
        public string SubscriptionClientName { get; }
    }
}
