DELETE [Products].[Order]
DELETE [System].[Users] WHERE [Roles] NOT LIKE N'%Admin%' AND [Roles] NOT LIKE N'%Province%' AND [Roles] NOT LIKE N'%District%' AND [Roles] NOT LIKE N'%Village%'
DELETE [Products].[AddressModel]
DELETE [Products].[Coupon_Promo_Code]
