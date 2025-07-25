﻿using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class CarFeature
    {
        [Key]
        public int FeatureId { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }

        public bool Status { get; set; } = true;
        public ICollection<CarListingFeature>? CarListingFeatures { get; set; }
    }
}
