﻿namespace ServiceInsight.SequenceDiagram.Diagram
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;
    using ServiceInsight.Models;

    [DebuggerDisplay("{Type}->{Name}")]
    public class Arrow : DiagramItem, IComparable<Arrow>
    {
        StoredMessage storedMessage;

        public Arrow(StoredMessage message, IMessageCommandContainer container)
        {
            storedMessage = message;

            CopyConversationIDCommand = container?.CopyConversationIDCommand;
            CopyMessageURICommand = container?.CopyMessageURICommand;
            RetryMessageCommand = container?.RetryMessageCommand;
            SearchByMessageIDCommand = container?.SearchByMessageIDCommand;
            ChangeCurrentMessage = container?.ChangeSelectedMessageCommand;
            ShowExceptionCommand = container?.ShowExceptionCommand;
        }

        public ICommand RetryMessageCommand { get; set; }

        public ICommand CopyConversationIDCommand { get; set; }

        public ICommand ShowExceptionCommand { get; set; }

        public ICommand CopyMessageURICommand { get; set; }

        public ICommand SearchByMessageIDCommand { get; set; }

        public ICommand ChangeCurrentMessage { get; set; }

        public StoredMessage SelectedMessage => storedMessage;

        public Handler FromHandler { get; set; }

        public Handler ToHandler { get; set; }

        public MessageProcessingRoute MessageProcessingRoute { get; set; }

        public Direction Direction { get; set; }

        public ArrowType Type { get; set; }

        public Endpoint Receiving => storedMessage.ReceivingEndpoint;

        public Endpoint Sending => storedMessage.SendingEndpoint;

        public DateTime? SentTime => storedMessage.TimeSent;

        public string MessageId => storedMessage.MessageId;

        public MessageStatus Status => storedMessage.Status;

        public double Width { get; set; }

        protected override void OnIsFocusedChanged()
        {
            base.OnIsFocusedChanged();
            if (Route != null)
            {
                Route.IsFocused = IsFocused;
            }
        }

        public MessageProcessingRoute Route { get; set; }

        public int CompareTo(Arrow other)
        {
            if (!other.SentTime.HasValue && !SentTime.HasValue)
            {
                return 0;
            }

            if (SentTime.HasValue && !other.SentTime.HasValue)
            {
                return 1;
            }

            if (!SentTime.HasValue)
            {
                return -1;
            }

            return SentTime.Value.CompareTo(other.SentTime.Value);
        }
    }
}