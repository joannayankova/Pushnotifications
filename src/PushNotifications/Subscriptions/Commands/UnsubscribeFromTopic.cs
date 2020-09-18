﻿using System;
using Elders.Cronus;
using System.Runtime.Serialization;

namespace PushNotifications.Subscriptions.Commands
{
    [DataContract(Name = "10c5a56c-8db5-4906-8306-4af56cf6fbda")]
    public class UnsubscribeFromTopic : ICommand
    {
        UnsubscribeFromTopic() { }

        public UnsubscribeFromTopic(TopicSubscriptionId id, SubscriberId subscriberId, Topic topic)
        {
            if (id is null) throw new ArgumentException(nameof(id));
            if (subscriberId is null) throw new ArgumentException(nameof(subscriberId));
            if (ReferenceEquals(null, topic)) throw new ArgumentNullException(nameof(topic));

            Id = id;
            SubscriberId = subscriberId;
            Topic = topic;
        }

        [DataMember(Order = 1)]
        public TopicSubscriptionId Id { get; private set; }

        [DataMember(Order = 2)]
        public SubscriberId SubscriberId { get; private set; }

        [DataMember(Order = 3)]
        public Topic Topic { get; private set; }
    }
}