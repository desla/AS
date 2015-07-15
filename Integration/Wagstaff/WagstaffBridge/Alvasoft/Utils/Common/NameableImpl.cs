using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alvasoft.Utils.Common {
   /// <summary>
   /// Реализация именуемого.
   /// </summary>
   /// <remarks>
   /// Базовый класс для реализации именуемого.
   /// </remarks>
   public class NameableImpl : Nameable {
      private string name;

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public NameableImpl() {
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
