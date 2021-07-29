using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelenaVlnaNewVersion.Models
{
    public class Cross
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
        public long Id { get; set; }
        //Kolikátá se křižovatka na cestě nachází
        public int Position { get; set; }

        //Délka cesty před křižovatkou
        public double PreviousIntervalLength { get; set; }
        //Patri do I?
        public bool BelongsToI { get; set; }
        //Patri do J?
        public bool BelongsToJ { get; set; }

        public Cross(long id, int position, double prevInt, bool isGreenOnA, bool isGreenOnB)
        {
            Id = id;
            Position = position;
            PreviousIntervalLength = prevInt;
            BelongsToI = isGreenOnA;
            BelongsToJ = isGreenOnB;
        }
    }
}
