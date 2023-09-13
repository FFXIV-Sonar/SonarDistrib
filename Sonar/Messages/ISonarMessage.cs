using MessagePack;
using Sonar.Models;
using Sonar.Config;
using System.Collections.Generic;
using Sonar.Logging;
using Sonar.Data.Details;
using Sonar.Trackers;
using Sonar.Relays;

namespace Sonar.Messages
{
    // System messages (Server to Client)
    [Union(0x00, typeof(ServerReady))]
    [Union(0x01, typeof(SonarLogMessage))] // Log Messages
    [Union(0x02, typeof(SonarMessage))] // Server in-game chat messages

    // Session Information Updates (Client to Server)
    [Union(0x10, typeof(PlayerInfo))]
    [Union(0x11, typeof(PlayerPosition))]
    [Union(0x12, typeof(SonarConfig))]
    [Union(0x13, typeof(SonarVersion))]
    //[Union(0x14, typeof(ClientModifiersOld))]
    [Union(0x15, typeof(ClientModifiers))]
    [Union(0x1A, typeof(ClientHello))]

    // Authentication and Verification (TODO)
    //[Union(0x20, typeof(AuthLogin))]
    //[Union(0x21, typeof(AuthRegister))]
    //[Union(0x22, typeof(AuthChange))]
    //[Union(0x23, typeof(VerificationRequest))]
    // 0x24 AuthResult
    // 0x25 VerificationResult
    [Union(0x29, typeof(ClientSecret))]
    [Union(0x2A, typeof(ClientIdentifier))]
    [Union(0x2B, typeof(HardwareIdentifier))]
    //[Union(0x2C, typeof(object))] // RESERVED: XivAuth?
    //[Union(0x2D, typeof(object))] // RESERVED: XivAuth?
    //[Union(0x2E, typeof(object))] // RESERVED: XivAuth?
    //[Union(0x2F, typeof(object))] // RESERVED: XivAuth?

    // Relays (Client to Server)
    [Union(0x30, typeof(HuntRelay))]
    [Union(0x31, typeof(FateRelay))]
    [Union(0x3f, typeof(ManualRelay))]

    // Relay States (Server to Client)
    [Union(0x40, typeof(RelayState<HuntRelay>))]
    [Union(0x41, typeof(RelayState<FateRelay>))]

    // Lock on requests
    [Union(0x140, typeof(LockOn<HuntRelay>))]
    [Union(0x141, typeof(LockOn<FateRelay>))]

    // Support / Communication
    [Union(0x50, typeof(SupportMessage))]
    [Union(0x51, typeof(SupportResponse))]

    // Relay Confirmation Requests
    [Union(0x60, typeof(RelayConfirmationRequest<HuntRelay>))]
    [Union(0x61, typeof(RelayConfirmationRequest<FateRelay>))]

    // Relay Confirmation Responses
    [Union(0x70, typeof(RelayConfirmationResponse<HuntRelay>))]
    [Union(0x71, typeof(RelayConfirmationResponse<FateRelay>))]

    // Unified and simplified implementation of relay confirmations
    [Union(0x62, typeof(RelayConfirmationSlim<HuntRelay>))]
    [Union(0x63, typeof(RelayConfirmationSlim<FateRelay>))]

    // Client Requests
    [Union(0x80, typeof(RelayDataRequest))]
    //[Union(0x81, typeof(RelayDataIndexCapacities))]

    // Database handling
    [Union(0xb0, typeof(SonarDbInfo))]
    [Union(0xb1, typeof(SonarDb))]

    // Diagnostic (Both ways)
    [Union(0xd0, typeof(SonarPing))]
    [Union(0xd1, typeof(SonarPong))]

    // Maintenance
    //[Union(0xe0, typeof(TimeSyncMessageOld))]
    [Union(0xe1, typeof(SonarHeartbeat))]

    // Multiple Messages (Warning: Potentially infinitely recursive. Infinite recursion is avoided in the MessageList itself but the possibility is still there.)
    [Union(0xff, typeof(MessageList))]

    public interface ISonarMessage
    {
        // Intentionally left empty
        // Identify using a switch case - https://github.com/neuecc/MessagePack-CSharp#union
    }

    public static class ISonarMessageExtensions
    {
        public static MessageList ConvertToMessageList<T>(this IEnumerable<T> source) where T : ISonarMessage
        {
            return MessageList.CreateFrom(source);
        }

        public static byte[] SerializeData(this ISonarMessage msg) => SonarSerializer.SerializeData(msg);
        public static byte[] SerializeClientToServer(this ISonarMessage msg) => SonarSerializer.SerializeClientToServer(msg);
        public static byte[] SerializeServerToClient(this ISonarMessage msg) => SonarSerializer.SerializeServerToClient(msg);
        public static byte[] SerializeFeed(this ISonarMessage msg) => SonarSerializer.SerializeFeed(msg);

        public static ISonarMessage IMessageFromData(this byte[] bytes) => SonarSerializer.DeserializeData<ISonarMessage>(bytes);
        public static ISonarMessage IMessageFromClient(this byte[] bytes) => SonarSerializer.DeserializeClientToServer<ISonarMessage>(bytes);
        public static ISonarMessage IMessageFromServer(this byte[] bytes) => SonarSerializer.DeserializeServerToClient<ISonarMessage>(bytes);
        public static ISonarMessage IMessageFromFeed(this byte[] bytes) => SonarSerializer.DeserializeFeed<ISonarMessage>(bytes);
    }
}
