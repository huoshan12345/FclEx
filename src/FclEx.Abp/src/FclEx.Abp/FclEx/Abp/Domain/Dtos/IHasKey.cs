using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FclEx.Abp.Domain.Dtos;

public interface IHasKey<TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
}