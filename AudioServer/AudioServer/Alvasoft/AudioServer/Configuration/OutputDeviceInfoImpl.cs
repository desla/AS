using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration {
   /// <summary>
   /// Реализация описания устройства вывода.
   /// </summary>
   public class OutputDeviceInfoImpl : IdentifiableNameableImpl, OutputDeviceInfo {
      private List<OutChannelInfoImpl> channels = new List<OutChannelInfoImpl>();

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public OutputDeviceInfoImpl() {
      }

      /// <inheritdoc />
      public int GetChannelsCount() {
         return channels.Count();
      }

      /// <inheritdoc />
      public OutChannelInfo GetChannel( int aIndex ) {
         return channels.ElementAt( aIndex );
      }

      /// <returns>Каналы.</returns>
      public List<OutChannelInfoImpl> GetChannels() {
         return channels;
      }
   }
}
