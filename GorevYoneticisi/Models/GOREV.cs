//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GorevYoneticisi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class GOREV
    {
        public int GorevID { get; set; }
        [Required(ErrorMessage = "Bu Alan Gereklidir")]
        public string Gorev_Ismi { get; set; }
        public string Gorev_Detay { get; set; }
        [Required(ErrorMessage = "Bu Alan Gereklidir")]
        public Nullable<System.DateTime> Baslangic_Tarihi { get; set; }
        public Nullable<System.DateTime> Son_Tarih { get; set; }
        public Nullable<bool> Tamamlandi_Mi { get; set; }
        public string Gorev_Tipi { get; set; }
        public Nullable<int> UserID { get; set; }
    
        public virtual KULLANICI KULLANICI { get; set; }
    }
}