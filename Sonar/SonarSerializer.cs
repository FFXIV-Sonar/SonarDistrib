using System.IO;
using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// TODO: Improve the interface to not always create new arrays. While most (99%) of use-cases are less than 80KB, there's still the allocation overhead to consider
namespace Sonar
{
    /// <summary>
    /// Handles serialization of all sonar messages sent between client and server and data resources
    /// </summary>
    public static class SonarSerializer
    {
        public static readonly MessagePackSerializerOptions MessagePackOptions = MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance)
            .WithCompression(MessagePackCompression.None) // Compression is handled separately
            .WithSecurity(MessagePackSecurity.TrustedData.WithMaximumObjectGraphDepth(100));

        /// <summary>
        /// Serializes a message to be sent from client to server
        /// </summary>
        public static byte[] SerializeClientToServer<T>(T msg) => MessagePackSerializer.Serialize(msg, MessagePackOptions);
        public static void SerializeClientToServer<T>(Stream outStream, T msg) => MessagePackSerializer.Serialize(outStream, msg, MessagePackOptions);
        /// <summary>
        /// Serializes a message to be sent from server to client
        /// </summary>
        public static byte[] SerializeServerToClient<T>(T msg) => MessagePackSerializer.Serialize(msg, MessagePackOptions).CompressToBrotli(8, 16);
        // Used to be (5, 22), however after benchmarking and generating time tables, the results of this research are as follow:
        // - Under Windows, highest safe numbers are (8, 16). Any higher on any of the 2 values will increase compression times 10x.
        //   Q0-3 are unaffected but does little compression.
        // - Under Linux, highest safe numbers are (9, 16). Increasing the window increase compression time 3-4x, however increasing
        //   quality will increase it 10x.
        // - These were concluded to be dependent upon which OS is running, however it could also be the difference between compilers or some other unknown variable
        // - As such, the safest value (tested on my machine (windows host and linux host), home server, large server, sonar server) are (8, 16) and therefore choosen.
        // - 8,16 has slightly better compression ratio than 5,22 with minimal additional overhead (or in case of windows, far reduced overhead).
        // - Sonar Server runs under Linux.
        // - Important to note is that Clients never compress messages before sending them to the server. Only the server does when sending them to clients.
        // - Database updates are compressed under maximum settings (using SerializeData) and cached. The fact that is cached means that it only does it once
        //   and sent them to clients as is.

        /// <summary>
        /// Serializes a message for data resources
        /// </summary>
        public static byte[] SerializeData<T>(T msg) => MessagePackSerializer.Serialize(msg, MessagePackOptions).CompressToBrotli(11, 24);
        /// <summary>
        /// Serializes a message to be sent to the raw feed
        /// </summary>
        public static byte[] SerializeFeed<T>(T msg) => MessagePackSerializer.Serialize(msg, MessagePackOptions);
        public static void SerializeFeed<T>(Stream outStream, T msg) => MessagePackSerializer.Serialize(outStream, msg, MessagePackOptions);

        /// <summary>
        /// Deserializes a message received from client to server
        /// </summary>
        public static T DeserializeClientToServer<T>(byte[] msg) => MessagePackSerializer.Deserialize<T>(msg, MessagePackOptions);
        /// <summary>
        /// Deserializes a message received from server to client
        /// </summary>
        public static T DeserializeServerToClient<T>(byte[] msg) => MessagePackSerializer.Deserialize<T>(msg.DecompressFromBrotli(), MessagePackOptions);
        /// <summary>
        /// Deserializes a message from data resources
        /// </summary>
        public static T DeserializeData<T>(byte[] msg) => MessagePackSerializer.Deserialize<T>(msg.DecompressFromBrotli(), MessagePackOptions);
        /// <summary>
        /// Deserializes a message received from the raw seed
        /// </summary>
        public static T DeserializeFeed<T>(byte[] msg) => MessagePackSerializer.Deserialize<T>(msg, MessagePackOptions);
    }
}
