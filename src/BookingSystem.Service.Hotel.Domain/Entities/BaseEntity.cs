using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Service.Hotel.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public bool IsDeleted { get; protected set; }

        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

    }
}
