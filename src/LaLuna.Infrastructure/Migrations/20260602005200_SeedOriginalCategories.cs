using LaLuna.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaLuna.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260602005200_SeedOriginalCategories")]
    public partial class SeedOriginalCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "Categories" ("Name", "Slug", "ImageUrl", "ParentId", "CreatedAt", "UpdatedAt")
                VALUES
                    ('Cosmeticos', 'lubrificantes', 'assets/images/categories/cosmeticos.svg', NULL, NOW(), NOW()),
                    ('Lingerie', 'lingeries', 'assets/images/categories/lingerie.svg', NULL, NOW(), NOW()),
                    ('Vibradores', 'vibradores', 'assets/images/categories/vibradores.svg', NULL, NOW(), NOW()),
                    ('Para Eles', 'para-eles', 'assets/images/categories/para-eles.svg', NULL, NOW(), NOW()),
                    ('Sexo Anal', 'sexo-anal', 'assets/images/categories/sexo-anal.svg', NULL, NOW(), NOW()),
                    ('Brincadeiras', 'brincadeiras', 'assets/images/categories/brincadeiras.svg', NULL, NOW(), NOW()),
                    ('Pompoarismo', 'pompoarismo', 'assets/images/categories/pompoarismo.svg', NULL, NOW(), NOW()),
                    ('SADO', 'sado', 'assets/images/categories/sado.svg', NULL, NOW(), NOW()),
                    ('Protese peniana', 'protese-peniana', 'assets/images/categories/protese-peniana.svg', NULL, NOW(), NOW())
                ON CONFLICT ("Slug") DO UPDATE
                SET
                    "Name" = EXCLUDED."Name",
                    "ParentId" = EXCLUDED."ParentId",
                    "ImageUrl" = EXCLUDED."ImageUrl",
                    "UpdatedAt" = NOW();
                """);

            migrationBuilder.Sql("""
                WITH parent_categories AS (
                    SELECT "Id", "Slug"
                    FROM "Categories"
                    WHERE "Slug" IN ('lubrificantes', 'lingeries', 'vibradores', 'para-eles', 'sexo-anal', 'brincadeiras')
                ),
                category_seed ("Name", "Slug", "ImageUrl", "ParentSlug") AS (
                    VALUES
                        ('Caldas Comestiveis', 'caldas-comestiveis', NULL, 'lubrificantes'),
                        ('Lubrificantes a Base D''agua', 'lubrificantes-base-agua', NULL, 'lubrificantes'),
                        ('Lubrificantes a Base de Silicone', 'lubrificantes-base-silicone', NULL, 'lubrificantes'),
                        ('Adstringentes', 'adstringentes', NULL, 'lubrificantes'),
                        ('Anestesico Anal', 'anestesico-anal', NULL, 'lubrificantes'),
                        ('Anestesicos / Dessensibilizante', 'anestesicos-dessensibilizante', NULL, 'lubrificantes'),
                        ('Excitantes Unissex', 'excitantes-unissex', NULL, 'lubrificantes'),
                        ('Higiene / Corpo e Banho', 'higiene-corpo-banho', NULL, 'lubrificantes'),
                        ('Desodorante Intimo', 'desodorante-intimo', NULL, 'lubrificantes'),
                        ('Para Massagem', 'para-massagem', NULL, 'lubrificantes'),
                        ('Suplementos / Energeticos', 'suplementos-energeticos', NULL, 'lubrificantes'),
                        ('Velas para Massagens', 'velas-para-massagens', NULL, 'lubrificantes'),
                        ('Vibradores Liquidos', 'vibradores-liquidos', NULL, 'lubrificantes'),
                        ('Lingeries', 'lingeries-subcategoria', NULL, 'lingeries'),
                        ('Lingeries Comestiveis', 'lingeries-comestiveis', NULL, 'lingeries'),
                        ('Cha de Lingerie', 'cha-de-lingerie', NULL, 'lingeries'),
                        ('Sado / Fetiche', 'sado-fetiche', NULL, 'lingeries'),
                        ('Calcinhas', 'calcinhas', NULL, 'lingeries'),
                        ('Fantasias', 'fantasias', NULL, 'lingeries'),
                        ('Estimulador de clitoris', 'estimulador-de-clitoris', NULL, 'vibradores'),
                        ('Preservativos', 'preservativos', NULL, 'para-eles'),
                        ('Masturbadores', 'masturbadores', NULL, 'para-eles'),
                        ('Bomba Peniana', 'bomba-peniana', NULL, 'para-eles'),
                        ('Retardantes de Ejaculacao', 'retardantes-de-ejaculacao', NULL, 'para-eles'),
                        ('Capa Peniana', 'capa-peniana', NULL, 'para-eles'),
                        ('Cuecas', 'cuecas', NULL, 'para-eles'),
                        ('Proteses', 'proteses', NULL, 'sexo-anal'),
                        ('Plugs', 'plugs', NULL, 'sexo-anal'),
                        ('Ducha Higienica', 'ducha-higienica', NULL, 'sexo-anal'),
                        ('Baralhos', 'baralhos', NULL, 'brincadeiras'),
                        ('Dados', 'dados', NULL, 'brincadeiras'),
                        ('SADO', 'sado', 'assets/images/categories/sado.svg', 'brincadeiras'),
                        ('Protese peniana', 'protese-peniana', 'assets/images/categories/protese-peniana.svg', 'para-eles')
                )
                INSERT INTO "Categories" ("Name", "Slug", "ImageUrl", "ParentId", "CreatedAt", "UpdatedAt")
                SELECT
                    category_seed."Name",
                    category_seed."Slug",
                    category_seed."ImageUrl",
                    parent_categories."Id",
                    NOW(),
                    NOW()
                FROM category_seed
                INNER JOIN parent_categories ON parent_categories."Slug" = category_seed."ParentSlug"
                ON CONFLICT ("Slug") DO UPDATE
                SET
                    "Name" = EXCLUDED."Name",
                    "ParentId" = EXCLUDED."ParentId",
                    "ImageUrl" = EXCLUDED."ImageUrl",
                    "UpdatedAt" = NOW();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "Categories"
                WHERE "Slug" IN (
                    'lubrificantes',
                    'lingeries',
                    'vibradores',
                    'para-eles',
                    'sexo-anal',
                    'brincadeiras',
                    'pompoarismo',
                    'sado',
                    'protese-peniana',
                    'caldas-comestiveis',
                    'lubrificantes-base-agua',
                    'lubrificantes-base-silicone',
                    'adstringentes',
                    'anestesico-anal',
                    'anestesicos-dessensibilizante',
                    'excitantes-unissex',
                    'higiene-corpo-banho',
                    'desodorante-intimo',
                    'para-massagem',
                    'suplementos-energeticos',
                    'velas-para-massagens',
                    'vibradores-liquidos',
                    'lingeries-subcategoria',
                    'lingeries-comestiveis',
                    'cha-de-lingerie',
                    'sado-fetiche',
                    'calcinhas',
                    'fantasias',
                    'estimulador-de-clitoris',
                    'preservativos',
                    'masturbadores',
                    'bomba-peniana',
                    'retardantes-de-ejaculacao',
                    'capa-peniana',
                    'cuecas',
                    'proteses',
                    'plugs',
                    'ducha-higienica',
                    'baralhos',
                    'dados'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM "Products"
                    WHERE "Products"."CategoryId" = "Categories"."Id"
                );
                """);
        }
    }
}
