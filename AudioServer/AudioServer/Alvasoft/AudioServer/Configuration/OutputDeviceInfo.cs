using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alvasoft.Utils.Common;

namespace Alvasoft.AudioServer.Configuration {
   /// <summary>
   /// Описание устройства вывода.
   /// <para>Идентификатор (<see cref="Alvasoft.Utils.Common.Identifiable.GetId"/>),
   /// является уникальным, адресуемым свойством для устройства вывода.</para>
   /// <para>Имя (<see cref="Alvasoft.Utils.Common.Nameable.GetName"/>),
   /// является уникальным, адресуемым свойством для устройства вывода.</para>
   /// </summary>
   public interface OutputDeviceInfo : IdentifiableNameable {
      /// <summary>
      /// Возвращает количество описаний каналов.
      /// </summary>
      /// <returns>Количество описаний каналов.</returns>
      int GetChannelsCount();

      /// <summary>
      /// Возвращает описание канала по индексу.
      /// </summary>
      /// <param name="aIndex">Индекс.</param>
      /// <returns>Описание канала.</returns>
      /// <exception cref="IndexOutOfRangeException">Неверный индекс. Допустимый диаппазон значений [0 .. <see cref="GetChannelsCount()"/> - 1]></exception>
      OutChannelInfo GetChannel( int aIndex );
   }
}
