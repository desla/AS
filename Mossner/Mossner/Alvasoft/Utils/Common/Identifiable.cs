using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Common {
   /// <summary>
   /// Идентифицируемое (имеющее идентификатор).
   /// </summary>
   public interface Identifiable {
       /// <summary>
       /// Возращает идентификатор.
       /// </summary>
       /// <returns>Идентификатор.</returns>
       long GetId();

      /// <summary>
      /// Изменяет идентификатор.
      /// </summary>
      /// <param name="aId">Идентификатор.</param>
      void SetId( long aId );
   }
}
