--开启CLR集成
EXEC sp_configure 'clr enabled','1'

--重新装载
RECONFIGURE

CREATE ASSEMBLY ItemPlugin
FROM 'D:\MSSQLDLL\SQLSERVERDLL2.dll'


GO

CREATE FUNCTION dbo.clrHelloWorld     
(     
    @str as nvarchar(2000)     
)      
RETURNS nvarchar(2000)    
 AS EXTERNAL NAME ItemPlugin.[SQLSERVERDLL2.HelloWorld].Hello   


GO

--开启CLR集成
EXEC sp_configure 'clr enabled','0'

--重新装载
RECONFIGURE

--测试
SELECT dbo.clrHelloWorld('Mary')




------------------------------------------------------------
------------------------------------------------------------
--


--开启CLR集成
EXEC sp_configure 'clr enabled','1'

--重新装载
RECONFIGURE

CREATE ASSEMBLY ItemPlugin2
FROM 'D:\MSSQLDLL\TCSOA4MSSQL.dll'


GO

CREATE FUNCTION dbo.clrHelloWorld2     
(     
    @str as nvarchar(2000)     
)      
RETURNS nvarchar(2000)    
 AS EXTERNAL NAME ItemPlugin2.[TC_SOA4MSSQL.ITEM].CreateItem   


GO

--开启CLR集成
EXEC sp_configure 'clr enabled','0'

--重新装载
RECONFIGURE

--测试
SELECT dbo.clrHelloWorld2('Mary')




SELECT A.FNumber+'/'+A.FFullName act,A.FDetailID,CONVERT(VARCHAR(100),IC.FItemClassID) itemClassNumber FROM dbo.t_Account A
LEFT JOIN dbo.t_ItemDetailV V ON V.FDetailID = A.FDetailID
LEFT JOIN dbo.t_ItemClass IC ON IC.FItemClassID = V.FItemClassID
WHERE FDelete=0 AND FDetail = 1 AND A.FNumber = '5201.21' 