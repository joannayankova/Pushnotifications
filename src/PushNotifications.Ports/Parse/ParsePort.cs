﻿using System;
using System.Linq;
using Elders.Cronus.DomainModeling;
using Elders.Cronus.DomainModeling.Projections;
using PushNotifications.Contracts.PushNotifications.Events;
using PushSharp;

namespace PushNotifications.Ports.Parse
{
    public class ParsePort : IPort, IPushNotificationPort, IHaveProjectionsRepository,
        IEventHandler<PushNotificationWasSent>
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ParsePort));

        public IPublisher<ICommand> CommandPublisher { get; set; }

        public IRepository Repository { get; set; }

        public PushBroker PushBroker { get; set; }

        public void Handle(PushNotificationWasSent @event)
        {
            var tokens = Repository.Query<ParseSubscriptions>().GetCollection(@event.UserId);

            var distinctTokens = tokens.Select(x => x.Token).Distinct().ToList();

            if (distinctTokens.Count == 0)
            {
                log.Info("There is no parse token for user with id: " + @event.UserId);

                return;
            }

            foreach (var token in distinctTokens)
            {
                try
                {
                    PushBroker.QueueNotification(new ParseAndroidNotifcation(token, @event.Json));

                    log.Info("Parse push notification was sent to device with token: " + token);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }
    }
}
