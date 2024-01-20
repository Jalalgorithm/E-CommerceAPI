using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class PopulateCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Categories(Name) VALUES ('Electronics') ");
            migrationBuilder.Sql("INSERT INTO Categories(Name) VALUES ('Home Accessories') ");
            migrationBuilder.Sql("INSERT INTO Categories(Name) VALUES ('Home and Beauty') ");
            migrationBuilder.Sql("INSERT INTO Categories(Name) VALUES ('Fashion') ");
            migrationBuilder.Sql("INSERT INTO Categories(Name) VALUES ('Meals and Drinks') ");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Categories WHERE Name IN ('Electronics' ,'Home Accessories' , 'Home and Beauty' , 'Fashion' , 'Meals and Drinks')");
        }
    }
}
