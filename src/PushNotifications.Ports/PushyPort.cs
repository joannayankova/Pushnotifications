﻿using Elders.Cronus.DomainModeling;
using Elders.Cronus.DomainModeling.Projections;
using PushNotifications.Contracts.PushNotifications.Delivery;
using PushNotifications.Contracts.PushNotifications.Events;
using PushNotifications.Delivery.Bulk;
using PushNotifications.Delivery.Pushy;
using PushNotifications.Ports.Logging;
using PushNotifications.Projections.Pushy;

namespace PushNotifications.Ports
{
    public class PushyPort : IPort,
        IEventHandler<PushNotificationSent>
    {
        static ILog log = LogProvider.GetLogger(typeof(PushyPort));

        public IPublisher<ICommand> CommandPublisher { get; set; }

        public IProjectionRepository Projections { get; set; }

        public BulkDelivery<PushyDelivery> PushyDelivery { get; set; }

        public void Handle(PushNotificationSent @event)
        {
            var projectionReponse = Projections.Get<SubscriberTokensForPushyProjection>(@event.SubscriberId);
            if (projectionReponse.Success == false)
                return;

            foreach (var token in projectionReponse.Projection.State.Tokens)
            {
                var notification = new NotificationDelivery(@event.NotificationPayload, @event.ExpiresAt, @event.ContentAvailable);
                PushyDelivery.Send(token, notification);
            }
        }
    }
}
