using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmapTest
{
    class Order
    {
        public string Id { set; get; }
        public string Name { set; get; }
        public string Lat { set; get; }
        public string Lon { set; get; }
        public DateTime DateCreate { set; get; }
        public string Description { set; get; }
        public string City { set; get; }
        public string Street { set; get; }
        public string House { set; get; }
        public string Flat { set; get; }
        public string Office { set; get; }
        public string Porch { set; get; }
        public string Floor { set; get; }
        public bool Intercom { set; get; }
        public bool InWork { set; get; }
        public DateTime DateOrder { set; get; }
        public string TimeBeg { set; get; }
        public string TimeEnd { set; get; }
        public string Phone1 { set; get; }
        public string Phone2 { set; get; }

        public Order(string id, string name, string lat, string lon,/* DateTime dateCreate,*/ string description, string city, string street, string house,
             string flat, string office, string porch, string floor,  /*bool intercom, bool inWork,DateTime dateOrder,*/ string timeBeg,
             string timeEnd, string phone1, string phone2)
        {
            Id = id;
            Name = name;
            Lat = lat;
            Lon = lon;
            //DateCreate = dateCreate;
            Description = description;
            City = city;
            Street = street;
            House = house;
            Flat = flat;
            Office = office;
            Porch = porch;
            Floor = floor;
            //Intercom = intercom;
            //InWork = inWork;
            //DateOrder = dateOrder;
            TimeBeg = timeBeg;
            TimeEnd = timeEnd;
            Phone1 = phone1;
            Phone2 = phone2;
        }


    }


}
