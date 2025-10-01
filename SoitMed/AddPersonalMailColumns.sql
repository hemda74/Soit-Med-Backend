-- Add PersonalMail column to AspNetUsers table (for ApplicationUser)
ALTER TABLE [AspNetUsers] 
ADD [PersonalMail] nvarchar(max) NULL;

-- Add PersonalMail column to Engineers table
ALTER TABLE [Engineers] 
ADD [PersonalMail] nvarchar(200) NULL;
