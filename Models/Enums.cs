using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gestion_SalleClasseEDT.Models
{
    public enum CourseStatus
    {
        Cree,
        EnAttente,
        Planifie,
        EnCours,
        Suspendu,
        Termine,
        Archive
    }

    public enum CourseType
    {
        CM,
        TD,
        TP,
        Projet,
        Seminaire,
        Examen,
        Soutenance
    }

    public enum TeachingMode
    {
        Presentiel,
        EnLigne,
        Hybride
    }
}