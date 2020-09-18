﻿using Elders.Cronus;
using System.Runtime.Serialization;
using System;

namespace PushNotifications.Subscriptions.Events
{
    [DataContract(Name = "9e655fe8-75d2-4481-a358-aec9d24e9e3d")]
    public class UnSubscribed : IEvent
    {
        UnSubscribed() { }

        public UnSubscribed(SubscriptionId id, SubscriberId subscriberId, SubscriptionToken subscriptionToken)
        {
            if (id is null) throw new ArgumentException(nameof(id));
            if (subscriberId is null) throw new ArgumentException(nameof(subscriberId));
            if (SubscriptionToken.IsValid(subscriptionToken) == false) throw new ArgumentException(nameof(subscriptionToken));

            Id = id;
            SubscriberId = subscriberId;
            SubscriptionToken = subscriptionToken;
        }

        [DataMember(Order = 1)]
        public SubscriptionId Id { get; private set; }

        [DataMember(Order = 2)]
        public SubscriberId SubscriberId { get; private set; }

        [DataMember(Order = 3)]
        public SubscriptionToken SubscriptionToken { get; private set; }
    }
}