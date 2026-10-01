using System;
using System.Collections.Generic;
using System.Text;
using ReolmarkedetG12.Core.Models;

namespace ReolmarkedetG12.Core.Repositories
{
    public interface IItemRepository : IRepository<Item>
    {
        Item? GetByItemNumber(int itemNumber);
    }
}
