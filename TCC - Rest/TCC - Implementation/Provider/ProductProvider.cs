using ClassesDAO.DAO;
using System;
using System.Collections.Generic;
using System.Text;

namespace TCC___Implementation.Provider
{
    public class ProductProvider
    {

        public static CoordinateDAO Coordinate(int position)
        {
            if (position == 1) return new CoordinateDAO { X = 450M, Y = 170M, Z = 110M };
            else if (position == 2) return new CoordinateDAO { X = 250M, Y = 170M, Z = 110M };
            else if (position == 3) return new CoordinateDAO { X = 450M, Y = 0M, Z = 110M };
            else if (position == 4) return new CoordinateDAO { X = 250M, Y = 0M, Z = 110M };
            else return null;
        }
    }
}
