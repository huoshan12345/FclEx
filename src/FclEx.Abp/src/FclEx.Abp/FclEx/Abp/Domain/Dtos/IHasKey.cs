using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Abp.Domain.Dtos
{
    public interface IHasKey<TPrimaryKey>
    {
        TPrimaryKey Id { get; set; }
    }
}
