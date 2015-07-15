using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration {
   /// <summary>
   /// Реализация описания канала.
   /// </summary>
   public class OutputChannelInfoImpl : IdentifiableNameableImpl, OutputChannelInfo {
      private OutputDeviceInfo device;
      private int channelNumber;
      private ChannelGroupInfo group;
      private long deviceId;
      private long groupId;

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public OutputChannelInfoImpl() {
         device = null;
         channelNumber = 0;
         group = null;
         deviceId = 0;
         groupId = 0;
      }

      /// <inheritdoc />
      public OutputDeviceInfo GetDevice() {
         return device;
      }

      /// <summary>
      /// Изменяет устройство вывода.
      /// </summary>
      /// <param name="aDevice">Устройство вывода.</param>
      public void SetDevice( OutputDeviceInfo aDevice ) {
         device = aDevice;
      }

      /// <inheritdoc />
      public int GetChannelNumber() {
         return channelNumber;
      }

      /// <summary>
      /// Изменяет номер канала.
      /// </summary>
      /// <param name="aChannelNumber">Номер канала.</param>
      public void SetChannelNumber( int aChannelNumber ) {
         channelNumber = aChannelNumber;
      }

      /// <inheritdoc />
      public ChannelGroupInfo GetGroup() {
         return group;
      }

      /// <summary>
      /// Изменяет группу каналов.
      /// </summary>
      /// <param name="aGroup">Группа каналов.</param>
      public void SetGroup( ChannelGroupInfo aGroup ) {
         group = aGroup;
      }

      /// <summary>
      /// Возвращает идентификатор устройства вывода.
      /// </summary>
      /// <returns>Номер устройства вывода.</returns>
      public long GetDeviceId() {
         return deviceId;
      }

      /// <summary>
      /// Изменяет идентификатор устройства.
      /// </summary>
      /// <param name="aDeviceId">Идентификатор устройства.</param>
      public void SetDeviceId( long aDeviceId ) {
         deviceId = aDeviceId;
      }

      /// <summary>
      /// Возвращает идентификатор группы каналов.
      /// </summary>
      /// <returns>Идентификатор группы каналов.</returns>
      public long GetGroupId() {
         return groupId;
      }

      /// <summary>
      /// Изменяет идентификатор группы каналов.
      /// </summary>
      /// <param name="aGroupId">Идентификатор группы каналов.</param>
      public void SetGroupId( long aGroupId ) {
         groupId = aGroupId;
      }
   }
}
