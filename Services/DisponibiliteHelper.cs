using System;
using Gestion_SalleClasseEDT.Models;

namespace Gestion_SalleClasseEDT.Services
{
    public static class DisponibiliteHelper
    {
        public static string GetDayCode(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "MON",
                DayOfWeek.Tuesday => "TUE",
                DayOfWeek.Wednesday => "WED",
                DayOfWeek.Thursday => "THU",
                DayOfWeek.Friday => "FRI",
                DayOfWeek.Saturday => "SAT",
                DayOfWeek.Sunday => "SUN",
                _ => string.Empty
            };
        }

        public static bool IsActiveForDate(DisponibiliteProf disponibilite, DateTime date)
        {
            if (disponibilite == null) return false;

            if (disponibilite.TypeDisponibilite == TypeDisponibilite.Ponctuelle)
            {
                if (disponibilite.DateSpecifique.HasValue)
                {
                    return disponibilite.DateSpecifique.Value.Date == date.Date;
                }

                return !string.IsNullOrWhiteSpace(disponibilite.JourSemaine) && disponibilite.JourSemaine == date.ToString("yyyy-MM-dd");
            }

            var dayCode = GetDayCode(date);
            return !string.IsNullOrWhiteSpace(disponibilite.JourSemaineCode) && disponibilite.JourSemaineCode.Equals(dayCode, StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(disponibilite.JourSemaine) && disponibilite.JourSemaine.Equals(dayCode, StringComparison.OrdinalIgnoreCase);
        }
    }
}
