using System;
using System.Collections.Generic;
using System.Text;

namespace ClassesDAO.DAO
{
    public class ProductDAO
    {
        public string Id { get; set; }
        public string Name{ get; set; }
        public int ArrivalDateDay { get; set; }
        public int ArrivalDateMonth { get; set; }
        public int ArrivalDateYear { get; set; }
        public int DeliveryDateDay { get; set; }
        public int DeliveryDateMonth { get; set; }
        public int DeliveryDateYear { get; set; }
        public bool Validated{ get; set;}
        public CoordinateDAO Coordinate{ get; set; }
        public int Position { get; set; }
        public string Color { get; set; }
    }
}
