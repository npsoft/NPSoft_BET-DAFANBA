DELETE [Products].[Order];
DELETE [System].[Users] WHERE [Roles] NOT IN (N'[Admin]', N'[Province]', N'[District]', N'[Village]');
DELETE [Products].[AddressModel];
DELETE [Products].[Coupon_Promo_Code];