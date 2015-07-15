using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration {
   /// <summary>
   /// Реализация описания группы каналов.
   /// </summary>
   public class ChannelGroupInfoImpl : IdentifiableImpl, ChannelGroupInfo {
      private List<OutChannelInfoImpl> channels = new List<OutChannelInfoImpl>();

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public ChannelGroupInfoImpl() {
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
