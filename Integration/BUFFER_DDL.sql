USE [WagstaffBuffer]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[opcBufferTable](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[type_id] [int] NOT NULL,
	[object_id] [int] NOT NULL,
	[data_id] [int] NOT NULL,
	[value_time] [datetime] NOT NULL,
	[value] [float] NOT NULL,
	[type_name] [varchar](50) NOT NULL,
	[object_name] [varchar](50) NOT NULL,
	[data_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_opcBufferTable] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


