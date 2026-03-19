using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideGo_Repository.Migrations
{
    /// <inheritdoc />
    public partial class PaymentOneToManyBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE payments DROP CONSTRAINT IF EXISTS payments_booking_id_fkey;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_payments_booking_id;");
            migrationBuilder.Sql("ALTER TABLE payments DROP COLUMN IF EXISTS booking_id;");
            migrationBuilder.Sql("ALTER TABLE bookings ADD COLUMN IF NOT EXISTS payment_id uuid;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_bookings_payment_id ON bookings (payment_id);");
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_bookings_payments_payment_id'
    ) THEN
        ALTER TABLE bookings
        ADD CONSTRAINT fk_bookings_payments_payment_id
        FOREIGN KEY (payment_id)
        REFERENCES payments (id)
        ON DELETE SET NULL;
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE bookings DROP CONSTRAINT IF EXISTS fk_bookings_payments_payment_id;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_bookings_payment_id;");
            migrationBuilder.Sql("ALTER TABLE bookings DROP COLUMN IF EXISTS payment_id;");
            migrationBuilder.Sql("ALTER TABLE payments ADD COLUMN IF NOT EXISTS booking_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS ix_payments_booking_id ON payments (booking_id);");
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_payments_bookings_booking_id'
    ) THEN
        ALTER TABLE payments
        ADD CONSTRAINT fk_payments_bookings_booking_id
        FOREIGN KEY (booking_id)
        REFERENCES bookings (id)
        ON DELETE CASCADE;
    END IF;
END $$;");
        }
    }
}
