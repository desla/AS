using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Common {
   /// <summary>
   /// Именуемое (имеющее имя).
   /// </summary>
   public interface Nameable {
      /// <summary>
      /// Возвращает имя.
      /// </summary>
      /// <returns>Имя.</returns>
      string GetName();

      /// <summary>
      /// Изменяет имя.
      /// </summary>
      /// <param name="aName">Имя.</param>
      void SetName( string aName );
   }
}
