using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration {
   /// <summary>
   /// Описание канала.
   /// <para>Идентификатор (<see cref="Alvasoft.Utils.Common.Identifiable.GetId"/>),
   /// является уникальным, адресуемым свойством.</para>
   /// <para>Имя (<see cref="Alvasoft.Utils.Common.Nameable.GetName"/>),
   /// является уникальным, адресуемым свойством.</para>
   /// </summary>
   public interface OutputChannelInfo : IdentifiableNameable {
      /// <summary>
      /// Возвращает устройство вывода.
      /// <para>Устройство, которому физически пренадлежит канал.</para>
      /// </summary>
      /// <returns>Устройство вывода</returns>
      OutputDeviceInfo GetDevice();

      /// <summary>
      /// Возвращает номер канала.
      /// <para>Физический номер канала на устройстве. </para>
      /// </summary>
      /// <returns>Номер канала.</returns>
      int GetChannelNumber();

      /// <summary>
      /// Возвращает группу канала.
      /// </summary>
      /// <returns>Группа канала</returns>
      ChannelGroupInfo GetGroup();
   }
}
