using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Common {
   /// <summary>
   /// Реализация идентифицируемого и именуемого.
   /// </summary>
   public class IdentifiableNameableImpl : IdentifiableImpl, IdentifiableNameable {
      private string name;

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public IdentifiableNameableImpl() {
         name = "";
      }

      /// <inheritdoc />
      public string GetName() {
         return name;
      }

      /// <inheritdoc />
      public void SetName( string aName ) {
         name = aName;
      }
   }
}
