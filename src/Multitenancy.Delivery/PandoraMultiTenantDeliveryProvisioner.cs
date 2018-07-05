﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elders.Pandora;
using Multitenancy.Delivery.Serialization;
using PushNotifications.Aggregator.InMemory;
using PushNotifications.Contracts.PushNotifications.Delivery;
using PushNotifications.Contracts.Subscriptions;
using PushNotifications.Delivery.FireBase;
using PushNotifications.Delivery.Pushy;

namespace Multitenancy.Delivery
{
    public class PandoraMultiTenantDeliveryProvisioner : IDeliveryProvisioner
    {
        readonly ISet<MultiTenantStoreItem> store;

        readonly Pandora pandora;

        public PandoraMultiTenantDeliveryProvisioner(Pandora pandora)
        {
            if (ReferenceEquals(null, pandora) == true) throw new ArgumentNullException(nameof(pandora));

            this.store = new HashSet<MultiTenantStoreItem>();
            this.pandora = pandora;
            Initialize();
        }

        public IPushNotificationDelivery ResolveDelivery(SubscriptionType subscriptionType, NotificationForDelivery notification)
        {
            if (ReferenceEquals(null, subscriptionType) == true) throw new ArgumentNullException(nameof(subscriptionType));
            if (ReferenceEquals(null, notification) == true) throw new ArgumentNullException(nameof(notification));

            var tenant = notification.Id.Tenant;
            var storeItem = store.SingleOrDefault(x => x.Tenant == tenant && x.SubscriptionType == subscriptionType);

            if (ReferenceEquals(null, storeItem) == true) throw new NotSupportedException($"There is no registered delivery for type '{subscriptionType}' and tenant '{tenant}'");
            return storeItem.Delivery;
        }

        public IPushNotificationDelivery ResolveDelivery(SubscriptionType subscriptionType, string tenant)
        {
            if (ReferenceEquals(null, subscriptionType) == true) throw new ArgumentNullException(nameof(subscriptionType));
            if (string.IsNullOrEmpty(tenant)) throw new ArgumentNullException(nameof(tenant));

            var storeItem = store.SingleOrDefault(x => x.Tenant == tenant && x.SubscriptionType == subscriptionType);

            if (ReferenceEquals(null, storeItem) == true) throw new NotSupportedException($"There is no registered delivery for type '{subscriptionType}' and tenant '{tenant}'");
            return storeItem.Delivery;
        }

        void Initialize()
        {
            var firebaseUrl = pandora.Get("delivery_firebase_baseurl");
            var firebaseSettings = pandora.Get<List<FireBaseSettings>>("delivery_firebase_settings");

            var pushyUrl = pandora.Get("delivery_pushy_baseurl");
            var pushySettings = pandora.Get<List<PushySettings>>("delivery_pushy_settings");

            foreach (var fb in firebaseSettings)
            {
                RegisterFireBaseDelivery(firebaseUrl, fb);
            }

            foreach (var pushy in pushySettings)
            {
                RegisterPushyDelivery(pushyUrl, pushy);
            }
        }

        void RegisterFireBaseDelivery(string baseUrl, FireBaseSettings settings)
        {
            if (string.IsNullOrEmpty(baseUrl) == true) throw new ArgumentNullException(nameof(baseUrl));
            if (settings.RecipientsCountBeforeFlush >= 1000) throw new ArgumentException($"FireBase limits the number of tokens to 1000. Use lower number for '{nameof(settings.RecipientsCountBeforeFlush)}'");

            var timeSpanBeforeFlush = TimeSpan.FromSeconds(settings.TimeSpanBeforeFlushInSeconds);
            var recipientsCountBeforeFlush = settings.RecipientsCountBeforeFlush;

            var fireBaseRestClient = new RestSharp.RestClient(baseUrl);
            var fireBaseDelivery = new FireBaseDelivery(fireBaseRestClient, NewtonsoftJsonSerializer.Default(), settings.ServerKey);
            fireBaseDelivery.UseAggregator(new InMemoryPushNotificationAggregator(fireBaseDelivery.Send, timeSpanBeforeFlush, recipientsCountBeforeFlush));

            store.Add(new MultiTenantStoreItem(settings.Tenant, SubscriptionType.FireBase, fireBaseDelivery));
        }

        void RegisterPushyDelivery(string baseUrl, PushySettings settings)
        {
            if (string.IsNullOrEmpty(baseUrl) == true) throw new ArgumentNullException(nameof(baseUrl));

            var timeSpanBeforeFlush = TimeSpan.FromSeconds(settings.TimeSpanBeforeFlushInSeconds);
            var recipientsCountBeforeFlush = settings.RecipientsCountBeforeFlush;

            var pushyRestClient = new RestSharp.RestClient(baseUrl);
            var pushyDelivery = new PushyDelivery(pushyRestClient, NewtonsoftJsonSerializer.Default(), settings.ServerKey);
            pushyDelivery.UseAggregator(new InMemoryPushNotificationAggregator(pushyDelivery.Send, timeSpanBeforeFlush, recipientsCountBeforeFlush));

            store.Add(new MultiTenantStoreItem(settings.Tenant, SubscriptionType.Pushy, pushyDelivery));
        }

        class PushySettings
        {
            public long TimeSpanBeforeFlushInSeconds { get; set; }

            public int RecipientsCountBeforeFlush { get; set; }

            public string ServerKey { get; set; }

            public string Tenant { get; set; }
        }

        class FireBaseSettings
        {
            public long TimeSpanBeforeFlushInSeconds { get; set; }

            public int RecipientsCountBeforeFlush { get; set; }

            public string ServerKey { get; set; }

            public string Tenant { get; set; }
        }
    }
}
