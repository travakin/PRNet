using System;
using System.Collections.Generic;
using System.Text;

namespace PRNet.Utils {
    public static class Identification {

        public static int GetUniqueIdentifierFromList(List<int> identifiers) {

            int id = -1;

            Random random = new Random();

            do id = random.Next();
            while (identifiers.Contains(id) || id < 0);

            return id;
        }

        public static int GetUniqueIdentifierFromList(List<int> identifiers, int max) {

            int id = -1;

            Random random = new Random();

            do id = random.Next(0, max);
            while (identifiers.Contains(id) || id < 0);

            return id;
        }

        public static int GetUniqueIdentifierFromList(List<int> identifiers, int min, int max) {

            int id = -1;

            Random random = new Random();

            do id = random.Next(min, max);
            while (identifiers.Contains(id) || id < 0);

            return id;
        }
    }
}
