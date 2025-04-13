using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProducerService.Models;

namespace ProducerService.Services
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(Post postData);
    }
}
