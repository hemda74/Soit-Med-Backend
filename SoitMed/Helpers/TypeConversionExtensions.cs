using System;
using SoitMed.Models.Enums;
using SoitMed.Models.Equipment;
using SoitMed.Models.Payment;

namespace SoitMed.Helpers
{
    public static class TypeConversionExtensions
    {
        public static string ToStringSafe(this Guid? guid) => guid?.ToString() ?? string.Empty;
        public static string ToStringSafe(this Guid guid) => guid.ToString();
        public static string ToStringSafe(this EquipmentStatus status) => status.ToString();
        public static string ToStringSafe(this RepairStatus status) => status.ToString();
        public static string ToStringSafe(this RepairPriority priority) => priority.ToString();
        public static string ToStringSafe(this PaymentStatus status) => status.ToString();
        public static string ToStringSafe(this PaymentMethod method) => method.ToString();
        
        public static EquipmentStatus ToEquipmentStatus(this string status) 
            => Enum.TryParse<EquipmentStatus>(status, out var result) ? result : EquipmentStatus.Operational;
        public static RepairStatus ToRepairStatus(this string status) 
            => Enum.TryParse<RepairStatus>(status, out var result) ? result : RepairStatus.Pending;
        public static RepairPriority ToRepairPriority(this string priority) 
            => Enum.TryParse<RepairPriority>(priority, out var result) ? result : RepairPriority.Medium;
        public static PaymentStatus ToPaymentStatus(this string status) 
            => Enum.TryParse<PaymentStatus>(status, out var result) ? result : PaymentStatus.Pending;
        public static PaymentMethod ToPaymentMethod(this string method) 
            => Enum.TryParse<PaymentMethod>(method, out var result) ? result : PaymentMethod.Cash;
    }
}
