declare @companyid int = 1046 
 

SET IDENTITY_INSERT [dbo].[tbl_Accounts] ON 
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (820, 0, N'1', N'الاصول', N'Asset', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (821, 820, N'101', N'اصول متداولة', N'Current Assets', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (822, 821, N'10101', N'الصناديق', N'Cash', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (823, 821, N'10102', N'البنوك', N'Banks', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (824, 821, N'10103', N'المخزون', N'Inventory', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (825, 821, N'10104', N'سلف', N'سلف', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), 1123, CAST(N'2024-04-01T11:39:30.267' AS DateTime), 0, NULL)
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (826, 821, N'10105', N'ذمم مدينة', N'ذمم مدينة', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), 1111, CAST(N'2023-03-01T16:39:38.633' AS DateTime), NULL, NULL)
 
INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (840, 821, N'10106', N'ذمم مدينه اخرى', N'Other receivables ', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (846, 821, N'10107', N'شيكات برسم التحصيل', N'شيكات برسم التحصيل', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (847, 846, N'1010701', N'شيكات برسم التحصيل-اوراق قبض', N'شيكات برسم التحصيل-اوراق قبض', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (848, 821, N'10108', N'ايرادات مستحقة ', N'ايرادات مستحقة ', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (850, 821, N'10109', N'مصاريف مدفوعة مقدما', N'مصاريف مدفوعة مقدما', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (851, 850, N'1010901', N'مصاريف مدفوعة مقدما', N'مصاريف مدفوعة مقدما', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (859, 821, N'10111', N'موجودات اخرى', N'موجودات اخرى', 2, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (871, 0, N'2', N'الالتزامات ', N'Liabilities ', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (872, 871, N'201', N'الالتزامات المتداولة', N'current liabilities', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (873, 872, N'20101', N'ذمم دائنة داخلية', N'ذمم دائنة داخلية', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (875, 872, N'20102', N'ذمم دائنة اخرى', N'ذمم دائنة اخرى', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (877, 875, N'2010202', N'ايداعات بنكية معلقة', N'ايداعات بنكية معلقة', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (878, 872, N'20103', N'مصاريف مستحقة', N'مصاريف مستحقة', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (880, 878, N'2010302', N'اتعاب تدقيق مستحقة', N'اتعاب تدقيق مستحقة', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (881, 878, N'2010303', N'مصاريف مكافات مستحقة', N'مصاريف مكافات مستحقة', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (883, 878, N'2010305', N'مصاريف رواتب مستحقة', N'مصاريف رواتب مستحقة', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (914, 0, N'3', N'حقوق الملكية', N'حقوق الملكية', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (915, 914, N'301', N'حقوق المساهمين', N'حقوق المساهمين', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (916, 915, N'30101', N'استثمارات اعضاء', N'استثمارات اعضاء', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (920, 915, N'30103', N'الاسهم المدفوعة', N'الاسهم المدفوعة', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (948, 915, N'30105', N'احتياطيات', N'احتياطيات', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (949, 948, N'3010501', N'احتياطي اجباري', N'احتياطي اجباري', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (951, 915, N'30104', N'الفائض', N'الفائض', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), 1123, CAST(N'2024-04-01T12:38:14.290' AS DateTime), 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (952, 951, N'3010401', N'الفائض الصافي', N'الفائض الصافي', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (953, 951, N'3010402', N'ارباح معدة للتوزيع', N'ارباح معدة للتوزيع', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (956, 951, N'3010405', N'مخصص ضريبة دخل', N'مخصص ضريبة دخل', 2, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (958, 0, N'4', N'الايرادات', N'الايرادات', 1, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (959, 958, N'401', N'المبيعات', N'المبيعات', 1, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (960, 959, N'40101', N'المبيعات', N'المبيعات', 1, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (961, 958, N'402', N'ايرادات رئيسية', N'ايرادات رئيسية', 1, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (970, 958, N'403', N'ايرادات استثمارات', N'ايرادات استثمارات', 1, 1050, 2, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), 1123, CAST(N'2024-04-02T10:05:49.417' AS DateTime), 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (972, 0, N'5', N'المشتريات', N'المشتريات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (973, 972, N'501', N'المشتريات', N'المشتريات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (974, 0, N'6', N'المصاريف', N'المصاريف', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (975, 974, N'601', N'مصاريف ادارية و عمومية', N'مصاريف ادارية و عمومية', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (976, 975, N'60101', N'مصاريف رواتب تسويق و تحصيل', N'مصاريف رواتب تسويق و تحصيل', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (977, 975, N'60102', N'مصاريف تنقلات', N'مصاريف تنقلات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (978, 975, N'60103', N'مصاريف ضيافة', N'مصاريف ضيافة', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (979, 975, N'60104', N'مصاريف بنكية', N'مصاريف بنكية', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (980, 975, N'60105', N'مصاريف اخرى', N'مصاريف اخرى', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (981, 975, N'60106', N'مصاريف نثرية', N'مصاريف نثرية', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), 1111, CAST(N'2025-02-20T10:18:34.650' AS DateTime), 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (982, 975, N'60107', N'مصاريف صيانة', N'مصاريف صيانة', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (983, 975, N'60108', N'مصاريف اتعاب محاسبة', N'مصاريف اتعاب محاسبة', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (984, 975, N'60109', N'مصاريف تبرعات', N'مصاريف تبرعات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (985, 975, N'60110', N'مصاريف مكافات', N'مصاريف مكافات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (986, 975, N'60111', N'مصاريف خصم هدية', N'مصاريف خصم هدية', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (987, 975, N'60112', N'مصاريف ضريبة غير قابلة للخصم', N'مصاريف ضريبة غير قابلة للخصم', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (988, 975, N'60113', N'مصاريف قرطاسية', N'مصاريف قرطاسية', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (989, 975, N'60114', N'مصاريف تراخيص و حكومية', N'مصاريف تراخيص و حكومية', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (990, 975, N'60115', N'مصاريف قطع كمبيوتر', N'مصاريف قطع كمبيوتر', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (992, 975, N'60117', N'مصاريف مكتب (اجرة،كهرباء،مياه،نت)', N'مصاريف مكتب (اجرة،كهرباء،مياه،نت)', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), 1111, CAST(N'2025-06-24T01:28:50.307' AS DateTime), 0, 0)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (993, 975, N'60118', N'مصاريف اتعاب و استشارات', N'مصاريف اتعاب و استشارات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (995, 975, N'60120', N'مصاريف خدمات المؤسسة التعاونية', N'مصاريف خدمات المؤسسة التعاونية', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (996, 975, N'60121', N'مصاريف اجتماعات', N'مصاريف اجتماعات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (997, 975, N'60122', N'مصاريف قضايا و محاكم', N'مصاريف قضايا و محاكم', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1007, 974, N'602', N'مصاريف اهتلاك', N'مصاريف اهتلاك', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1008, 1007, N'60201', N'مصاريف استهلاك اثاث', N'مصاريف استهلاك اثاث', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1009, 1007, N'60202', N'مصاريف استهلاك اجهزة كمبيوتر', N'مصاريف استهلاك اجهزة كمبيوتر', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1010, 1007, N'60203', N'مصاريف استهلاك برامج كمبيوتر', N'مصاريف استهلاك برامج كمبيوتر', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1011, 1007, N'60204', N'مصاريف استهلاك اجهزة و معدات', N'مصاريف استهلاك اجهزة و معدات', 1, 1050, 1, 0, CAST(N'2023-03-01T12:21:52.450' AS DateTime), NULL, NULL, NULL, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1012, 872, N'20105', N'ذمم دائنة', N'ذمم دائنة', 2, 1050, 2, 1111, CAST(N'2023-03-01T16:40:01.267' AS DateTime), 1123, CAST(N'2026-02-03T00:23:36.727' AS DateTime), 0, 0)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1024, 878, N'2010307', N'اتعاب محاسبة مستحقة', N'اتعاب محاسبة مستحقة', 2, 1050, 2, 1111, CAST(N'2024-01-29T22:20:47.327' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1028, 820, N'102', N'اصول ثابتة', N'Fixed Assets', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:19:07.983' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1029, 1028, N'10201', N'اثاث ومفروشات', N'اثاث ومفروشات', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:42:09.497' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1030, 1028, N'10202', N'كمبيوترات', N'كمبيوترات', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:42:46.640' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1031, 1028, N'10203', N'برامج الكمبيوتر', N'برامج الكمبيوتر', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:43:09.463' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1032, 1028, N'10204', N'اجهزة و معدات', N'اجهزة و معدات', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:43:29.220' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1033, 1028, N'10205', N'مجمع استهلاك الاثاث', N'مجمع استهلاك الاثاث', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:43:53.057' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1034, 1028, N'10206', N'مجمع استهلاك الكمبيوترات', N'مجمع استهلاك الكمبيوترات', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:44:15.663' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1035, 1028, N'10207', N'مجمع استهلاك برامج الكمبيوتر', N'مجمع استهلاك برامج الكمبيوتر', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:44:50.587' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1036, 1028, N'10208', N'مجمع استهلاك اجهزة و معدات', N'مجمع استهلاك اجهزة و معدات', 2, 1050, 1, 1123, CAST(N'2024-04-01T11:45:12.617' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1040, 872, N'20106', N'امانات اعضاء', N'امانات اعضاء', 2, 1050, 2, 1123, CAST(N'2024-04-01T12:28:18.333' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1056, 824, N'1010301', N'بضاعة اخر المدة', N'بضاعة اخر المدة', 2, 1050, 1, 1123, CAST(N'2024-04-02T10:10:26.737' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1059, 915, N'30106', N'جاري المساهمين', N'جاري المساهمين', 2, 1050, 2, 1111, CAST(N'2025-01-01T16:42:04.017' AS DateTime), 1111, CAST(N'2025-05-05T11:56:18.637' AS DateTime), 0, 1)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1060, 958, N'404', N'ايراد اشتراكات ', N'ايراد اشتراكات ', 1, 1050, 2, 1111, CAST(N'2025-01-01T17:01:56.397' AS DateTime), 1111, CAST(N'2025-08-11T22:57:12.067' AS DateTime), 0, 0)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1061, 1040, N'2010604', N'امانات المساهمين', N'امانات المساهمين', 2, 1050, 2, 1111, CAST(N'2025-01-01T17:13:26.000' AS DateTime), 1111, CAST(N'2025-05-06T23:01:38.417' AS DateTime), 0, 1)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1067, 958, N'405', N'ايرادات اخرى', N'ايرادات اخرى', 1, 1050, 2, 1123, CAST(N'2025-02-09T11:48:30.507' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1068, 1067, N'40501', N'ايرادات اخرى', N'ايرادات اخرى', 1, 1050, 2, 1123, CAST(N'2025-02-09T11:48:52.820' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1069, 975, N'60123', N'مصاريف ايجار', N'مصاريف ايجار', 1, 1050, 1, 1123, CAST(N'2025-02-09T11:58:59.947' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1070, 975, N'60124', N'مصاريف فائدة مدينة', N'مصاريف فائدة مدينة', 1, 1050, 1, 1123, CAST(N'2025-02-09T11:59:18.780' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1071, 820, N'103', N'مخصص ديون مشكوك في تحصيلها', N'مخصص ديون مشكوك في تحصيلها', 2, 1050, 1, 1135, CAST(N'2025-02-17T09:53:44.290' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1072, 821, N'10112', N'العهد', N'العهد', 2, 1050, 1, 1111, CAST(N'2025-02-20T11:07:57.043' AS DateTime), NULL, NULL, 0, NULL)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1075, 1067, N'40502', N'ايراد تبرعات', N'ايراد تبرعات', 1, 1050, 2, 1123, CAST(N'2025-05-12T23:53:41.807' AS DateTime), NULL, NULL, 0, 0)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1077, 821, N'10113', N'ذمم موظفين', N'ذمم موظفين', 2, 1050, 1, 1123, CAST(N'2025-05-14T02:28:18.173' AS DateTime), NULL, NULL, 0, 1)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1081, 875, N'2010205', N'فروقات قيد التحميل', N'فروقات قيد التحميل', 2, 1050, 2, 1123, CAST(N'2025-06-24T00:48:14.603' AS DateTime), NULL, NULL, 0, 0)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1093, 878, N'2010308', N'تبرعات مستحقة', N'تبرعات مستحقة', 2, 1050, 2, 1123, CAST(N'2026-01-20T22:39:24.770' AS DateTime), NULL, NULL, 0, 0)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1098, 878, N'2010309', N'مصاريف فائدة مدينة مستحقة', N'مصاريف فائدة مدينة مستحقة', 2, 1050, 2, 1111, CAST(N'2026-02-03T23:04:57.487' AS DateTime), NULL, NULL, 0, 0)

INSERT [dbo].[tbl_Accounts] ([ID], [ParentID], [AccountNumber], [AName], [EName], [ReportingTypeID], [CompanyID], [AccountNatureID], [CreationUserID], [CreationDate], [ModificationUserID], [ModificationDate], [ReportingTypeNodeID], [IsSubLedger]) VALUES (1099, 878, N'2010310', N'ضريبه المبيعات', N'ضريبه المبيعات', 2, 1050, 2, 1, CAST(N'2026-02-04T09:45:55.350' AS DateTime), NULL, NULL, 0, 0)

SET IDENTITY_INSERT [dbo].[tbl_Accounts] OFF

SET IDENTITY_INSERT [dbo].[tbl_AccountSetting] ON 

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (1, 1, 973, 1050, 1, CAST(N'2026-02-04T09:42:54.457' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (2, 2, 960, 1050, 1, CAST(N'2026-02-04T09:42:54.767' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (3, 3, 960, 1050, 1, CAST(N'2026-02-04T09:42:55.107' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (4, 4, 973, 1050, 1, CAST(N'2026-02-04T09:42:55.423' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (5, 5, 822, 1050, 1, CAST(N'2026-02-04T09:42:55.733' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (6, 6, 1012, 1050, 1, CAST(N'2026-02-04T09:42:56.160' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (7, 7, 826, 1050, 1, CAST(N'2026-02-04T09:42:56.453' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (8, 8, 1056, 1050, 1, CAST(N'2026-02-04T09:42:56.747' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (9, 9, 0, 1050, 1, CAST(N'2026-02-04T09:42:57.063' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (10, 10, 0, 1050, 1, CAST(N'2026-02-04T09:42:57.380' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (11, 11, 0, 1050, 1, CAST(N'2026-02-04T09:42:57.680' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (12, 12, 0, 1050, 1, CAST(N'2026-02-04T09:42:57.990' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (13, 13, 960, 1050, 1, CAST(N'2026-02-04T09:42:58.290' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (14, 14, 986, 1050, 1, CAST(N'2026-02-04T09:42:58.620' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (15, 15, 823, 1050, 1, CAST(N'2026-02-04T09:42:59.010' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (16, 16, 847, 1050, 1, CAST(N'2026-02-04T09:42:59.307' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (17, 17, 847, 1050, 1, CAST(N'2026-02-04T09:42:59.617' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (18, 18, 1077, 1050, 1, CAST(N'2026-02-04T09:42:59.937' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (19, 19, 1056, 1050, 1, CAST(N'2026-02-04T09:43:00.237' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (20, 20, 1056, 1050, 1, CAST(N'2026-02-04T09:43:00.547' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (21, 21, 1056, 1050, 1, CAST(N'2026-02-04T09:43:00.873' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (22, 22, 1056, 1050, 1, CAST(N'2026-02-04T09:43:01.157' AS DateTime), NULL, NULL, 0)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (23, 1, 973, 1050, 1, CAST(N'2026-02-04T09:46:27.710' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (24, 2, 960, 1050, 1, CAST(N'2026-02-04T09:46:27.957' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (25, 3, 960, 1050, 1, CAST(N'2026-02-04T09:46:28.217' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (26, 4, 973, 1050, 1, CAST(N'2026-02-04T09:46:28.590' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (27, 5, 822, 1050, 1, CAST(N'2026-02-04T09:46:28.937' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (28, 6, 1012, 1050, 1, CAST(N'2026-02-04T09:46:29.237' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (29, 7, 826, 1050, 1, CAST(N'2026-02-04T09:46:29.547' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (30, 8, 1056, 1050, 1, CAST(N'2026-02-04T09:46:29.850' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (31, 9, 1099, 1050, 1, CAST(N'2026-02-04T09:46:30.177' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (32, 10, 1099, 1050, 1, CAST(N'2026-02-04T09:46:30.453' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (33, 11, 1099, 1050, 1, CAST(N'2026-02-04T09:46:30.763' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (34, 12, 1099, 1050, 1, CAST(N'2026-02-04T09:46:31.107' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (35, 13, 960, 1050, 1, CAST(N'2026-02-04T09:46:31.397' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (36, 14, 986, 1050, 1, CAST(N'2026-02-04T09:46:31.797' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (37, 15, 823, 1050, 1, CAST(N'2026-02-04T09:46:32.107' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (38, 16, 847, 1050, 1, CAST(N'2026-02-04T09:46:32.417' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (39, 17, 847, 1050, 1, CAST(N'2026-02-04T09:46:32.727' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (40, 18, 1077, 1050, 1, CAST(N'2026-02-04T09:46:33.037' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (41, 19, 1056, 1050, 1, CAST(N'2026-02-04T09:46:33.333' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (42, 20, 1056, 1050, 1, CAST(N'2026-02-04T09:46:33.643' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (43, 21, 1056, 1050, 1, CAST(N'2026-02-04T09:46:34.077' AS DateTime), NULL, NULL, 1)

INSERT [dbo].[tbl_AccountSetting] ([ID], [AccountRefID], [AccountID], [CompanyID], [CreationUserID], [CreationDate], [ModificationUserID], [ModofocationDate], [Active]) VALUES (44, 22, 1056, 1050, 1, CAST(N'2026-02-04T09:46:34.363' AS DateTime), NULL, NULL, 1)

SET IDENTITY_INSERT [dbo].[tbl_AccountSetting] OFF

update tbl_AccountSetting set companyid = @companyid
update tbl_Accounts set companyid = @companyid
