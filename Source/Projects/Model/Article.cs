using System;

namespace Model
{
    public class Article
    {
        public Guid ArticleId { get; set; }
        public int PLU { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
