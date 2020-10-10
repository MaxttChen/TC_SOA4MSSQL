//==================================================
// 
//  Copyright 2017 Siemens Product Lifecycle Management Software Inc. All Rights Reserved.
//
//==================================================



using System;

using Teamcenter.ClientX;
using Teamcenter.Schemas.Soa._2006_03.Exceptions;

//Include the Saved Query Service Interface
using Teamcenter.Services.Strong.Query;

// Input and output structures for the service operations 
// Note: the different namespace from the service interface
using Teamcenter.Services.Strong.Query._2006_03.SavedQuery;

using Teamcenter.Services.Strong.Core;
using Teamcenter.Soa.Client.Model;

using ImanQuery = Teamcenter.Soa.Client.Model.Strong.ImanQuery;
using SavedQueriesResponse = Teamcenter.Services.Strong.Query._2007_09.SavedQuery.SavedQueriesResponse;
using QueryInput = Teamcenter.Services.Strong.Query._2008_06.SavedQuery.QueryInput;
using QueryResults = Teamcenter.Services.Strong.Query._2007_09.SavedQuery.QueryResults;

using System.Collections;


// Input and output structures for the service operations
// Note: the different namespace from the service interface
using Teamcenter.Services.Strong.Core._2006_03.DataManagement;
using Teamcenter.Services.Strong.Core._2007_01.DataManagement;
using Teamcenter.Services.Strong.Core._2008_06.DataManagement;

using Teamcenter.Soa.Exceptions;

using Item = Teamcenter.Soa.Client.Model.Strong.Item;
using ItemRevision = Teamcenter.Soa.Client.Model.Strong.ItemRevision;
using Teamcenter.Services.Strong.Query._2007_06.SavedQuery;
using Teamcenter.Services.Strong.Workflow;
using Teamcenter.Services.Strong.Workflow._2008_06.Workflow;

namespace TC_SOA_cmd
{
    public class DataManagement
    {

        public void createMyItem(string itemType)
        {
            try
            {

                // Get the service stub
                DataManagementService dmService = DataManagementService.getService(Session.getConnection());
                ObjectOwner objectOwner = new ObjectOwner();
                
                //根据物料号创建ITEMID
                ItemProperties itemProperty = new ItemProperties();

                itemProperty.ClientId = "Maxtt-Test-demo10";   //物料名称
                itemProperty.ItemId = "000092";   //物料代码
                itemProperty.RevId = "00";    //版本
                itemProperty.Name = "Maxtt-Test";    //物料名称
                itemProperty.Type = itemType;   //创建ITEM的类型
                itemProperty.Description = "Test Item for the SOA AppX sample application.Hello";    //描述
                //test
                itemProperty.Uom = "PCS";  //单位

                //增加额外属性
                itemProperty.ExtendedAttributes = new ExtendedAttributes[2];   //增加多少个？
                ExtendedAttributes[] theExtendedAttr = new ExtendedAttributes[2];

                //第1个
                theExtendedAttr[0] = new ExtendedAttributes();
                theExtendedAttr[0].Attributes = new Hashtable();
                theExtendedAttr[0].ObjectType = "Item Master";      //对应哪个form表
                theExtendedAttr[0].Attributes["project_id"] = "project_id";
                itemProperty.ExtendedAttributes[0] = theExtendedAttr[0];

                //第2个
                theExtendedAttr[1] = new ExtendedAttributes();
                theExtendedAttr[1].Attributes = new Hashtable();
                theExtendedAttr[1].ObjectType = "ItemRevision Master";      //对应哪个form表
                theExtendedAttr[1].Attributes["user_data_2"] = "data_2";
                itemProperty.ExtendedAttributes[1] = theExtendedAttr[1];

                

                //链接服务器创建
                CreateItemsResponse response = dmService.CreateItems(new ItemProperties[] { itemProperty }, null, "");

                //调用查询构建器，查询ITEM和ITEMRevision
                ModelObject itemObj = findModel("Item ID", new string[] { "Item ID" }, new string[] { itemProperty.ItemId });
                ModelObject itemReversion = findModel("MY_WEB_ITEM_REVISION", new string[] { "iid", "vid" }, new string[] { itemProperty.ItemId, itemProperty.RevId });


                //修改ITEM所有者
                //changeOnwer("maxtt", "项目管理", itemObj);
                //changeOnwer("maxtt", "项目管理", itemReversion);

                //新增版本--不能修改所有者不是infodba用户的ITEM
                //reviseItem(itemReversion);

                //修改原有的版本


                //deleteItems_single(itemReversion);

                //发布流程
                wf("MyRelease",itemReversion);

            }
            //catch (ServiceException e)
            catch (Exception e)
            {
                System.Console.Out.WriteLine(e.Message);
            }


        }

        public void wf(String wfTemplate,ModelObject obj)
        {

            //connection -> TC connection objects
            //WorkflowService is from package com.teamcenter.services.strong.workflow
            WorkflowService wfService = WorkflowService.getService(Session2.getConnection());

            if (wfService == null)
            {
                throw new Exception("The workflow service not found in Teamcenter.");
            }

            String[] arrObjectUID = new String[] { obj.Uid };
            int[] arrTypes = new int[arrObjectUID.Length];
            arrTypes[0] = 1;
            //Arrays.fill(arrTypes, 1);//Target attachment type to be initialized for all UIDs


            ContextData contextData = new ContextData();
            contextData.AttachmentCount = arrObjectUID.Length;//
            contextData.Attachments = arrObjectUID;//List of UID of objects to submit to workflow
            contextData.AttachmentTypes = arrTypes; //Types of attachment  EPM_target_attachment (target attachment) and EPM_reference_attachment (reference attachment).
            contextData.ProcessTemplate = wfTemplate;//"ReleaseObjectsWorkflow";
            
            //processname -> Name by which initiated workflow appear in the user mail box .
            //processDescription -> Description for the initiated workflow
            InstanceInfo instanceResponse = wfService.CreateInstance(true, null, "processName-maxtt-demo", null, "processDescription-maxtt-demo", contextData);

            if (instanceResponse.ServiceData.sizeOfPartialErrors() == 0)
            {
                //System.out.println("Process created Successfully");
            }
            else
            {
                //System.out.println("Error :" + instanceResponse.serviceData.getPartialError(0).getMessages());
                //System.out.println("Error :" + instanceResponse.serviceData.getPartialError(0).getErrorValues());
                //throw new Exception("Submit To Workflow: 001", "Submit To Workflow - " + instanceResponse.ServiceData.GetPartialError(0).GetMessages()[0]);
                throw new Exception("Submit To Workflow: 001"+ "Submit To Workflow - ");
            }
        }

        /**
         * Perform a sequence of data management operations: Create Items, Revise
         * the Items, and Delete the Items
         *
         */
        public void createReviseAndDelete()
        {
            try
            {
                int numberOfItems = 3;

                // Reserve Item IDs and Create Items with those IDs
                ItemIdsAndInitialRevisionIds[] itemIds = generateItemIds(numberOfItems, "Item");
                CreateItemsOutput[] newItems = createItems(itemIds, "Item");

                //// Copy the Item and ItemRevision to separate arrays for further
                //// processing
                //Item[] items = new Item[newItems.Length];
                //ItemRevision[] itemRevs = new ItemRevision[newItems.Length];
                //for (int i = 0; i < items.Length; i++)
                //{
                //    items[i] = newItems[i].Item;
                //    itemRevs[i] = newItems[i].ItemRev;
                //}



                // Reserve revision IDs and revise the Items
                //Hashtable allRevIds = generateRevisionIds(items);
                //reviseItems(allRevIds, itemRevs);

                // Delete all objects created
                //deleteItems(items);
            }
            catch (ServiceException e)
            {
                System.Console.Out.WriteLine(e.Message);
            }

        }

        /**
         * Reserve a number Item and Revision Ids
         *
         * @param numberOfIds      Number of IDs to generate
         * @param type             Type of IDs to generate
         *
         * @return An array of Item and Revision IDs. The size of the array is equal
         *         to the input numberOfIds
         *
         * @throws ServiceException   If any partial errors are returned
         */
        public ItemIdsAndInitialRevisionIds[] generateItemIds(int numberOfIds, String type)
        //        throws ServiceException
        {
            // Get the service stub
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());

            GenerateItemIdsAndInitialRevisionIdsProperties[] properties = new GenerateItemIdsAndInitialRevisionIdsProperties[1];
            GenerateItemIdsAndInitialRevisionIdsProperties property = new GenerateItemIdsAndInitialRevisionIdsProperties();

            property.Count = numberOfIds;
            property.ItemType = type;
            property.Item = null; // Not used
            properties[0] = property;

            // *****************************
            // Execute the service operation
            // *****************************
            GenerateItemIdsAndInitialRevisionIdsResponse response = dmService.GenerateItemIdsAndInitialRevisionIds(properties);



            // The AppXPartialErrorListener is logging the partial errors returned
            // In this simple example if any partial errors occur we will throw a
            // ServiceException
            if (response.ServiceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.generateItemIdsAndInitialRevisionIds returned a partial error.");

            // The return is a map of ItemIdsAndInitialRevisionIds keyed on the
            // 0-based index of requested IDs. Since we only asked for IDs for one
            // data type, the map key is '0'
            Int32 bIkey = 0;
            Hashtable allNewIds = response.OutputItemIdsAndInitialRevisionIds;

            ItemIdsAndInitialRevisionIds[] myNewIds = (ItemIdsAndInitialRevisionIds[])allNewIds[bIkey];

            return myNewIds;
        }

        /**
         * Create Items
         *
         * @param itemIds        Array of Item and Revision IDs
         * @param itemType       Type of item to create
         *
         * @return Set of Items and ItemRevisions
         *
         * @throws ServiceException  If any partial errors are returned
         */
        public CreateItemsOutput[] createItems(ItemIdsAndInitialRevisionIds[] itemIds, String itemType)
        //       throws ServiceException
        {
            // Get the service stub
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());
            // Populate form type
            GetItemCreationRelatedInfoResponse relatedResponse = dmService.GetItemCreationRelatedInfo(itemType, null);
            String[] formTypes = new String[0];
            if (relatedResponse.ServiceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.getItemCretionRelatedInfo returned a partial error.");

            formTypes = new String[relatedResponse.FormAttrs.Length];
            for (int i = 0; i < relatedResponse.FormAttrs.Length; i++)
            {
                FormAttributesInfo attrInfo = relatedResponse.FormAttrs[i];
                formTypes[i] = attrInfo.FormType;
            }

            ItemProperties[] itemProps = new ItemProperties[itemIds.Length];
            for (int i = 0; i < itemIds.Length; i++)
            {
                // Create form in cache for form property population
                ModelObject[] forms = createForms(itemIds[i].NewItemId, formTypes[0],
                                                  itemIds[i].NewRevId, formTypes[1],
                                                  null, false);
                ItemProperties itemProperty = new ItemProperties();

                itemProperty.ClientId = "AppX-Test";
                itemProperty.ItemId = itemIds[i].NewItemId;
                itemProperty.RevId = itemIds[i].NewRevId;
                itemProperty.Name = "AppX-Test";
                itemProperty.Type = itemType;
                itemProperty.Description = "Test Item for the SOA AppX sample application.";
                itemProperty.Uom = "";

                // Retrieve one of form attribute value from Item master form.
                ServiceData serviceData = dmService.GetProperties(forms, new String[] { "project_id" });
                if (serviceData.sizeOfPartialErrors() > 0)
                    throw new ServiceException("DataManagementService.getProperties returned a partial error.");
                Property property = null;
                try
                {
                    property = forms[0].GetProperty("project_id");
                }
                catch (NotLoadedException /*ex*/) { }


                // Only if value is null, we set new value
                //if (property == null || property.StringValue == null || property.StringValue.Length == 0)
                if (1==1)
                {
                    itemProperty.ExtendedAttributes = new ExtendedAttributes[1];

                    ExtendedAttributes theExtendedAttr = new ExtendedAttributes();
                    theExtendedAttr.Attributes = new Hashtable();
                    theExtendedAttr.ObjectType = formTypes[0];
                    theExtendedAttr.Attributes["project_id"] = "project_id";

                    itemProperty.ExtendedAttributes[0] = theExtendedAttr;
                }
                itemProps[i] = itemProperty;
            }


            // *****************************
            // Execute the service operation
            // *****************************
            CreateItemsResponse response = dmService.CreateItems(itemProps, null, "");
            // before control is returned the ChangedHandler will be called with
            // newly created Item and ItemRevisions



            // The AppXPartialErrorListener is logging the partial errors returned
            // In this simple example if any partial errors occur we will throw a
            // ServiceException
            if (response.ServiceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.createItems returned a partial error.");

            return response.Output;
        }


            /**
             * Reserve Revision IDs
             *
             * @param items       Array of Items to reserve Ids for
             *
             * @return Map of RevisionIds
             *
             * @throws ServiceException  If any partial errors are returned
            */
        public Hashtable generateRevisionIds(Item[] items) //throws ServiceException
        {
            // Get the service stub
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());

            GenerateRevisionIdsResponse response = null;
            GenerateRevisionIdsProperties[] input = null;
            input = new GenerateRevisionIdsProperties[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                GenerateRevisionIdsProperties property = new GenerateRevisionIdsProperties();
                property.Item = items[i];
                property.ItemType = "";
                input[i] = property;
            }

            // *****************************
            // Execute the service operation
            // *****************************
            response = dmService.GenerateRevisionIds(input);

            // The AppXPartialErrorListener is logging the partial errors returned
            // In this simple example if any partial errors occur we will throw a
            // ServiceException
            if (response.ServiceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.generateRevisionIds returned a partial error.");

            return response.OutputRevisionIds;
        }

        /**
         * Revise Items
         *
         * @param revisionIds     Map of Revsion IDs
         * @param itemRevs        Array of ItemRevisons
         *
         * @return Map of Old ItemRevsion(key) to new ItemRevision(value)
         *
         * @throws ServiceException         If any partial errors are returned
         */
        public void reviseItems(Hashtable revisionIds, ItemRevision[] itemRevs) //throws ServiceException
        {
            // Get the service stub
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());


            

            ReviseInfo[] reviseInfo = new ReviseInfo[itemRevs.Length];
            for (int i = 0; i < itemRevs.Length; i++)
            {

                RevisionIds rev = (RevisionIds)revisionIds[i];

                reviseInfo[i] = new ReviseInfo();
                reviseInfo[i].BaseItemRevision = itemRevs[i];
                reviseInfo[i].ClientId = itemRevs[i].Uid + "--" + i;
                reviseInfo[i].Description = "describe testRevise";
                reviseInfo[i].Name = "testRevise";
                reviseInfo[i].NewRevId = rev.NewRevId;
            }

            

            // *****************************
            // Execute the service operation
            // *****************************
            ReviseResponse2 revised = dmService.Revise2(reviseInfo);
            // before control is returned the ChangedHandler will be called with
            // newly created Item and ItemRevisions



            // The AppXPartialErrorListener is logging the partial errors returned
            // In this simple example if any partial errors occur we will throw a
            // ServiceException
            if (revised.ServiceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.revise returned a partial error.");


        }


        public void reviseItem(ModelObject obj) //throws ServiceException
        {

            DataManagementService dmService = DataManagementService.getService(Session.getConnection());
            ReviseInfo rev = new ReviseInfo();
            rev.BaseItemRevision = new ItemRevision(null,obj.Uid);
            rev.ClientId = "Maxtt_Test" + "--" + "01";
            rev.Description = "describe testRevise";
            rev.Name = "testRevise";
            rev.NewRevId = "01";
            
            //额外的表单属性
            PropertyNameValueInfo info = new PropertyNameValueInfo();
            info.PropertyName = "object_desc";
            info.PropertyValues = new string[] { "newid1" };
            
            rev.NewItemRevisionMasterProperties.PropertyValueInfo = new PropertyNameValueInfo[] { info };
            //rev.NewItemRevisionMasterProperties.Form = new Teamcenter.Soa.Client.Model.Strong.Form(null, obj.Uid);

            // *****************************
            // Execute the service operation
            // *****************************
            ReviseResponse2 revised = dmService.Revise2(new ReviseInfo[] { rev });
            // before control is returned the ChangedHandler will be called with
            // newly created Item and ItemRevisions



            // The AppXPartialErrorListener is logging the partial errors returned
            // In this simple example if any partial errors occur we will throw a

            // ServiceException
            if (revised.ServiceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.revise returned a partial error.");
        }

        /**
         * Delete the Items
         *
         * @param items     Array of Items to delete
         *
         * @throws ServiceException    If any partial errors are returned
         */
        public void deleteItems(Item[] items) //throws ServiceException
        {
            // Get the service stub
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());

            // *****************************
            // Execute the service operation
            // *****************************
            ServiceData serviceData = dmService.DeleteObjects(items);
            

            // The AppXPartialErrorListener is logging the partial errors returned
            // In this simple example if any partial errors occur we will throw a
            // ServiceException
            if (serviceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.deleteObjects returned a partial error.");

        }

        public void deleteItems_single(ModelObject items) //throws ServiceException
        {
            // Get the service stub
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());

            // *****************************
            // Execute the service operation
            // *****************************
            ServiceData serviceData = dmService.DeleteObjects(new ModelObject[] { items });


            // The AppXPartialErrorListener is logging the partial errors returned
            // In this simple example if any partial errors occur we will throw a
            // ServiceException
            if (serviceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.deleteObjects returned a partial error.");

        }

        /**
         * Create ItemMasterForm and ItemRevisionMasterForm
         *
         * @param IMFormName      Name of ItemMasterForm
         * @param IMFormType      Type of ItemMasterForm
         * @param IRMFormName     Name of ItemRevisionMasterForm
         * @param IRMFormType     Type of ItemRevisionMasterForm
         * @param parent          The container object that two
         *                        newly-created forms will be added into.
         * @return ModelObject[]  Array of forms
         *
         * @throws ServiceException         If any partial errors are returned
         */
        public ModelObject[] createForms(String IMFormName, String IMFormType,
                                    String IRMFormName, String IRMFormType,
                                    ModelObject parent, bool saveDB) //throws ServiceException
        {
            //Get the service stub
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());
            FormInfo[] inputs = new FormInfo[2];
            inputs[0] = new FormInfo();
            inputs[0].ClientId = "1";
            inputs[0].Description = "";
            inputs[0].Name = IMFormName;
            inputs[0].FormType = IMFormType;
            inputs[0].SaveDB = saveDB;
            inputs[0].ParentObject = parent;
            inputs[1] = new FormInfo();
            inputs[1].ClientId = "2";
            inputs[1].Description = "";
            inputs[1].Name = IRMFormName;
            inputs[1].FormType = IRMFormType;
            inputs[1].SaveDB = saveDB;
            inputs[1].ParentObject = parent;
            CreateOrUpdateFormsResponse response = dmService.CreateOrUpdateForms(inputs);
            if (response.ServiceData.sizeOfPartialErrors() > 0)
                throw new ServiceException("DataManagementService.createForms returned a partial error.");
            ModelObject[] forms = new ModelObject[inputs.Length];
            for (int i = 0; i < inputs.Length; ++i)
            {
                forms[i] = response.Outputs[i].Form;
            }
            return forms;
        }


        //重构查询器
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryName">查询构建器的主键名称</param>
        /// <param name="keys">查询条件key</param>
        /// <param name="values">查询 条件values</param>
        /// <returns></returns>
        public ModelObject findModel(String queryName,String[] keys,string[] values)
        {
            ImanQuery query = null;

            // Get the service stub
            SavedQueryService queryService = SavedQueryService.getService(Session.getConnection());
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());

            try
            {

                // *****************************
                // Execute the service operation
                // *****************************
                GetSavedQueriesResponse savedQueries = queryService.GetSavedQueries();


                if (savedQueries.Queries.Length == 0)
                {
                    return null;
                }

                // Find one called 'Item Name'
                for (int i = 0; i < savedQueries.Queries.Length; i++)
                {
                    
                    if (savedQueries.Queries[i].Name.Equals(queryName))
                    {
                        query = savedQueries.Queries[i].Query;
                        break;
                    }
                }
            }
            catch (ServiceException e)
            {
                return null;
            }

            if (query == null)
            {
                return null;
            }

            try
            {
                SavedQueryInput[] savedQueryInput = new SavedQueryInput[1];
                savedQueryInput[0] = new SavedQueryInput();
                savedQueryInput[0].Query = query;
                savedQueryInput[0].MaxNumToReturn = 25;
                savedQueryInput[0].LimitListCount = 0;
                savedQueryInput[0].LimitList = new Teamcenter.Soa.Client.Model.ModelObject[0];
                savedQueryInput[0].Entries = keys;//Attribute in Query to search by
                savedQueryInput[0].Values = values;
                savedQueryInput[0].MaxNumToReturn = 25;



                ExecuteSavedQueriesResponse savedQueryResult = queryService.ExecuteSavedQueries(savedQueryInput);
                SavedQueryResults found = savedQueryResult.ArrayOfResults[0];

                ModelObject[] modelObjs = found.Objects;


                return modelObjs[0];
            }
            catch (ServiceException e)
            {
                return null;
            }
        }



        public void changeOnwer(String userName,String groupName,ModelObject modl)
        {
            //ModelObject user = findUser(userName);
            ModelObject user = findModel("__WEB_find_user", new string[] { "User ID" }, new string[] { userName });
            if (null == user)
            {
                return;
            }

            //ModelObject userGroup = findGroup(groupName);
            ModelObject userGroup = findModel("__WEB_group", new string[] { "Name" }, new string[] { groupName });
            if (null == userGroup)
            {
                return;
            }

            DataManagementService dmService = DataManagementService.getService(Session.getConnection());
            ObjectOwner[] ownerData = new ObjectOwner[1];

            ObjectOwner ownrObj = new ObjectOwner();
            ownrObj.Object = modl;
            ownrObj.Group = (Teamcenter.Soa.Client.Model.Strong.Group) userGroup;
            ownrObj.Owner = (Teamcenter.Soa.Client.Model.Strong.User) user;
            ownerData[0] = ownrObj;


            ServiceData returnData = dmService.ChangeOwnership(ownerData);

            if (returnData.sizeOfPartialErrors() > 0)
            {

                throw new Exception("Change ownership service: 005" +  "Change ownership service - " );

            }
        }


        public void queryItems()
        {

            ImanQuery query = null;

            // Get the service stub
            SavedQueryService queryService = SavedQueryService.getService(Session.getConnection());
            DataManagementService dmService = DataManagementService.getService(Session.getConnection());

            try
            {

                // *****************************
                // Execute the service operation
                // *****************************
                GetSavedQueriesResponse savedQueries = queryService.GetSavedQueries();


                if (savedQueries.Queries.Length == 0)
                {
                    Console.Out.WriteLine("There are no saved queries in the system.");
                    return;
                }

                // Find one called 'Item Name'
                for (int i = 0; i < savedQueries.Queries.Length; i++)
                {

                    if (savedQueries.Queries[i].Name.Equals("MY_WEB_ITEM_REVISION"))
                    {
                        query = savedQueries.Queries[i].Query;
                        break;
                    }
                }
            }
            catch (ServiceException e)
            {
                Console.Out.WriteLine("GetSavedQueries service request failed.");
                Console.Out.WriteLine(e.Message);
                return;
            }

            if (query == null)
            {
                Console.WriteLine("There is not an 'Item Name' query.");
                return;
            }

            try
            {
                // Search for all Items, returning a maximum of 25 objects
                QueryInput[] savedQueryInput = new QueryInput[1];
                savedQueryInput[0] = new QueryInput();
                savedQueryInput[0].Query = query;
                savedQueryInput[0].MaxNumToReturn = 25;
                savedQueryInput[0].LimitList = new Teamcenter.Soa.Client.Model.ModelObject[0];
                //savedQueryInput[0].Entries = new String[] { "Name", "ItemID" };
                //savedQueryInput[0].Values = new String[2];
                //savedQueryInput[0].Values[0] = "Maxtt-Test";
                //savedQueryInput[0].Values[1] = "000090";

                savedQueryInput[0].Entries = new String[] { "iid","vid" };
                savedQueryInput[0].Values = new String[] { "*90","001" };

                //*****************************
                //Execute the service operation
                //*****************************
                SavedQueriesResponse savedQueryResult = queryService.ExecuteSavedQueries(savedQueryInput);
                QueryResults found = savedQueryResult.ArrayOfResults[0];


                System.Console.Out.WriteLine("");
                System.Console.Out.WriteLine("Found Items:");
                // Page through the results 10 at a time
                for (int i = 0; i < found.ObjectUIDS.Length; i += 10)
                {
                    int pageSize = (i + 10 < found.ObjectUIDS.Length) ? 10 : found.ObjectUIDS.Length - i;

                    String[] uids = new String[pageSize];
                    for (int j = 0; j < pageSize; j++)
                    {
                        uids[j] = found.ObjectUIDS[i + j];
                    }
                    ServiceData sd = dmService.LoadObjects(uids);
                    ModelObject[] foundObjs = new ModelObject[sd.sizeOfPlainObjects()];
                    for (int k = 0; k < sd.sizeOfPlainObjects(); k++)
                    {
                        foundObjs[k] = sd.GetPlainObject(k);
                    }

                    Session.printObjects(foundObjs);
                }
            }
            catch (ServiceException e)
            {
                Console.Out.WriteLine("ExecuteSavedQuery service request failed.");
                Console.Out.WriteLine(e.Message);
                return;
            }

        }

    }   

        
}

