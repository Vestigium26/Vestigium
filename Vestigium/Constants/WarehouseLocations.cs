namespace Vestigium.Constants
{
    public static class WarehouseLocations
    {
        // ==========================================
        // CONFIGURACIÓN DE RACKS Y NIVELES
        // ==========================================

        public static readonly IReadOnlyDictionary<string, int>
            RackLevels =
                new Dictionary<string, int>
                {
                    ["A"] = 5,
                    ["B"] = 5,
                    ["C"] = 5,
                    ["D"] = 4,
                    ["E"] = 4,
                    ["F"] = 3,
                    ["G"] = 4,
                    ["H"] = 5,
                    ["I"] = 3,
                    ["J"] = 4,
                    ["K"] = 3,
                    ["L"] = 3,
                    ["M"] = 3,
                    ["N"] = 3,
                    ["W"] = 4,
                    ["X"] = 5,
                    ["Y"] = 6
                };


        // ==========================================
        // OBTENER TODOS LOS RACKS
        // ==========================================

        public static List<string> GetRacks()
        {
            return RackLevels
                .Keys
                .OrderBy(x => x)
                .ToList();
        }


        // ==========================================
        // OBTENER NIVELES DE UN RACK
        // ==========================================

        public static List<int> GetLevels(
            string? rack)
        {
            if (
                string.IsNullOrWhiteSpace(rack) ||
                !RackLevels.TryGetValue(
                    rack.ToUpperInvariant(),
                    out var maxLevel)
            )
            {
                return new List<int>();
            }


            return Enumerable
                .Range(
                    1,
                    maxLevel)
                .ToList();
        }


        // ==========================================
        // OBTENER TODAS LAS UBICACIONES
        // ==========================================

        public static List<string> GetLocations()
        {
            var locations =
                new List<string>();


            foreach (
                var rack in RackLevels
                    .OrderBy(x => x.Key)
            )
            {
                for (
                    int level = 1;
                    level <= rack.Value;
                    level++
                )
                {
                    locations.Add(
                        $"{rack.Key}{level}");
                }
            }


            return locations;
        }


        // ==========================================
        // VALIDAR RACK
        // ==========================================

        public static bool IsValidRack(
            string? rack)
        {
            if (string.IsNullOrWhiteSpace(
                rack))
            {
                return false;
            }


            return RackLevels.ContainsKey(
                rack.Trim()
                    .ToUpperInvariant());
        }


        // ==========================================
        // VALIDAR RACK + NIVEL
        // ==========================================

        public static bool IsValid(
            string? rack,
            int? level)
        {
            if (
                string.IsNullOrWhiteSpace(rack) ||
                !level.HasValue
            )
            {
                return false;
            }


            var rackCode =
                rack.Trim()
                    .ToUpperInvariant();


            if (
                !RackLevels.TryGetValue(
                    rackCode,
                    out var maxLevel)
            )
            {
                return false;
            }


            return
                level.Value >= 1 &&
                level.Value <= maxLevel;
        }


        // ==========================================
        // VALIDAR CÓDIGO COMPLETO
        // Ejemplos: A1, B3, Y6
        // ==========================================

        public static bool IsValid(
            string? locationCode)
        {
            if (string.IsNullOrWhiteSpace(
                locationCode))
            {
                return false;
            }


            var value =
                locationCode
                    .Trim()
                    .ToUpperInvariant();


            if (value.Length < 2)
            {
                return false;
            }


            var rack =
                value.Substring(
                    0,
                    1);


            var levelText =
                value.Substring(
                    1);


            if (
                !int.TryParse(
                    levelText,
                    out var level)
            )
            {
                return false;
            }


            return IsValid(
                rack,
                level);
        }


        // ==========================================
        // CONSTRUIR CÓDIGO DE UBICACIÓN
        // ==========================================

        public static string? BuildLocationCode(
            string? rack,
            int? level)
        {
            if (!IsValid(
                rack,
                level))
            {
                return null;
            }


            return
                $"{rack!.Trim().ToUpperInvariant()}" +
                $"{level!.Value}";
        }
    }
}