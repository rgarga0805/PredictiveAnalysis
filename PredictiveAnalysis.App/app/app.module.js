/// <reference path="../Scripts/angular.min.js" />
(function () {
    'use strict';

    var myApp = angular.module('myModule', ['ui.bootstrap','ui.utils']);
    var myController = function ($scope, $sce, $http, $compile) {
        $scope.showgraph = false;
        $scope.result = '';
        $scope.someObject = true;


        //$scope.customerDisplayInfo = '';
        $scope.customerDisplayInfo = [];
        angular.element("#one").trigger("click");  
        //calculateChurnOnPageLoad();
        //function calculateChurnOnPageLoad()
        //{
            //var dataToSubmit = { Timeline: "Monthly", ValueFilter: "Yearly", Value: "2015", CustomerName: "FORD MOTOR", Inputs: [1007, 1011] };
            //$scope.showgraph = true;
            //callAnalyticsResult(dataToSubmit);
            
        //}

        $scope.calculateChurn = function (id)
        {
            $('.customerList').removeClass('highlightCustomer');
            var customerName = $('#' + id).attr("data-name");
            $('#Cust_' + id + 'kpi_' + id + '  :checkbox:checked')
            var selectedCustomer = '';
            angular.forEach($scope.ChurnModelInfo.Customers, function (value, key) {
                if (value.id == id)
                     selectedCustomer = value;
            });
            var selectedKPIs = [];
            if (selectedCustomer)
            {
              var customerId = selectedCustomer.id;
              $.each(selectedCustomer.KPIPerCustomer, function (key, value) {
                  var selectedoption = '';
                  if (value[customerId] && value[customerId][value.Id].checked)
                  {
                      selectedoption = value[customerId]
                      console.log(Object.keys(selectedoption)[0])
                      selectedKPIs.push(Object.keys(selectedoption)[0]);
                  }
                });
            }
            //return;

            //var dataToSubmit = { Timeline: "Monthly", ValueFilter: "Yearly", Value: $scope.year, CustomerName: customerName, Inputs: [1007, 1011] };
            if ($scope.year == '' || selectedKPIs.length <= 0)
            {
                alert("Year and atleast one KPI is mandatory to proceed");
                return;
            }
            var dataToSubmit = { Timeline: "Monthly", ValueFilter: "Yearly", Value: $scope.year, CustomerName: customerName, Inputs: selectedKPIs };
            //return;
           // $scope.selectedYear = {};
            $('#' + id).addClass('highlightCustomer');
            if (angular.element(document.querySelectorAll('.churnPeriod')).length > 0)
            {
                angular.element(document.querySelectorAll('.churnPeriod')).first().addClass('highlightCustomer');
            }
            callAnalyticsResult(dataToSubmit);
            $scope.showgraph = true;
        }

        var responsePromise = $http.get(CONFIG.get('GET_CUSTOMER_DETAILS')).then(function (response, status, headers, config) {
            var customerData = response.data;
           
            $http.get(CONFIG.get('GET_KPI_LIST')).then(function (response, status, headers, config) {
                customerInfo(customerData, response.data);
               // console.log("data is :" + JSON.stringify(response.data));
                $scope.year = "2015";
                var dataToSubmit = { Timeline: "Monthly", ValueFilter: "Yearly", Value: "2015", CustomerName: "", Inputs: [1007, 1011] };
            $scope.showgraph = true;
            callAnalyticsResult(dataToSubmit);
            $scope.year = '';
              //Get the all customer information
            },
                function (data, status, headers, config) {
                    alert(CONFIG.get('API_FAIL_TEXT'));
                });
        },
            function (data, status, headers, config) {
                alert(CONFIG.get('API_FAIL_TEXT'));
            });
        $scope.timeLine = ["2013", "2014", "2015"];
        $scope.changeSelectedItem= function(selectedYear)
        {
            $scope.year = selectedYear;
        }

        function customerInfo(customerData,KPIData)
        {
            var kpiinfo = KPIData;
           
            $scope.ChurnModelInfo =
                {
                    Customers: [],
                    Durarion: [
                        { displayName: 'Monthly', start: 1, till: 2 },
                        { displayName: 'Yearly', start: 2011, till: 2013 },
                    ]

                };
            var kpiData = [];
            angular.forEach(kpiinfo, function (value, key) {
                var data = new Object();
                data.Id = value.Id;
                data.DisplayName = value.DisplayName;
                if (key == 0)
                    data.checked = false;
                else
                    data.checked = true;
                kpiData.push(data);
            });
            
            angular.forEach(customerData, function (value, key) {
                var customer = new Object();
                customer.id = value.Id;
                customer.displayName = value.DisplayName;
                customer.logo = CUSTOMER_IMAGE.get(value.Id);
                customer.KPIPerCustomer = kpiData;
                $scope.ChurnModelInfo.Customers.push(customer);
                //KPIPerCustomer
            });
        }
        function callAnalyticsResult(data)
        {
            var test = $scope.selectedYear;
           // alert("ttt" + test);
            //$scope.dataToSubmit = { Timeline: "Monthly", ValueFilter: "Yearly", Value: $scope.selectedYear, CustomerName: "FORD MOTOR", Inputs: [1007, 1011] };
            $scope.dataToSubmit = data;
            $scope.postWPFData($scope.dataToSubmit, CONFIG.get('GET_ANALYTICS_URL'));
        }

        function prepareInputData()
        {
            //get the selected TimeLine Value

            //Get the selected Year from the drop down

           // var year = $scope.selectedYear.year;

            //Geet the seleected customer name

            //get the selected variable Ids


        }

        $scope.postWPFData = function (data, url) {
            var config = {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'Accept': 'application/json'
                }
            }
            var responsePromise = $http.post(url, $.param({ DATA: JSON.stringify(data) }), config).then(function (response, status, headers, config) {
                //sendEmail();

                $('.myDataTable').html('');
                var returnedInfo = response.data;
                var dataset = response.data.testData;
                //$scope.graphImage = returnedInfo.graphImage;
                $scope.churnRate = '';
               // $scope.churnRate = response.data.churnRate;
                $scope.churnRate = (((response.data.churnRate) * 100).toFixed(2));
                if ($scope.churnRate > 60)
                    $scope.churnPositive = 'YES'
                else
                    $scope.churnPositive = 'NO'
                $scope.monthWiseData = response.data.monthWiseData;
                var confusionMtrxResult = returnedInfo.validateModel;
                $scope.falsePositive = confusionMtrxResult.falsePositives;
                $scope.falseNegative = confusionMtrxResult.falseNegatives;
                $scope.truePositive = confusionMtrxResult.truePositives;
                $scope.trueNegative = confusionMtrxResult.trueNegatives;
                $scope.accuracy = ($scope.truePositive + $scope.trueNegative) / ($scope.falsePositive + $scope.falseNegative + $scope.truePositive + $scope.trueNegative)
                createGraphImage(response.data.xAxisDataPoints);
                createStateWiseChurnGraph(response.data.stateData.StateWiseChurn);
                createArrayDataTable(dataset);
                $scope.columnNames = response.data.columnNames;

            }, function (response, status, headers, config) {
                alert(CONFIG.get('API_FAIL_TEXT'));
            });
        }



        function createArrayDataTable(dataset)
        {
            $scope.data = [];
            $scope.customerDisplayInfo = [];
            angular.forEach(dataset, function (value, key) {
                $scope.customerDisplayInfo.push(value.ItemArray);
            });
            $scope.data = $scope.customerDisplayInfo;

            $scope.dataTableOpt = {
                //custom datatable options 
                // or load data through ajax call also
                "aLengthMenu": [[10, 50, 100, -1], [10, 50, 100, 'All']],
            };
            $('.dynamicTable').remove();
            var htmlToAppend = angular.element('<div class="container"><div class="panel" ><div class="panel-heading border"></div><div class="panel-body dynamicTable">');
            var compileAppendedArticleHTML =  $compile(htmlToAppend)
            var element = compileAppendedArticleHTML($scope);
            //$('.dtTable').html('');
            $('.dtTable').html(element)
            htmlToAppend = angular.element('<table class="table myDataTable table-bordered bordered table-striped table-condensed datatable"  ui-jq="dataTable" ui-options="dataTableOpt">');
            compileAppendedArticleHTML = $compile(htmlToAppend)
            element = compileAppendedArticleHTML($scope);
            $('.dynamicTable').html(element);
            $('.myDataTable').html('<thead><tr><th>#</th><th>Name</th><th>On Time Delivery</th><th>Total Net Revenue</th><th>Churn</th></tr></thead>');
           // $compile(test)($scope)
            htmlToAppend = angular.element('<tbody><tr ng-repeat="n in data"><td>{{$index+1}}</td><td>{{n[0]}}</td><td>{{n[1]}}</td><td>{{n[2]}}</td><td>{{n[3]}}</td></tr></tbody></table>');
            compileAppendedArticleHTML = $compile(htmlToAppend);
            element = compileAppendedArticleHTML($scope);
            $('.myDataTable').append(element);
        }
        

        $scope.callNPLResult = function()
        {
            var responsePromise = $http.get(CONFIG.get('GET_NPL_URL')).then(function (response, status, headers, config) {
                $scope.result = response.data;
                $scope.customerFeedbackReview = response.data.sentiment.document.score;
                $scope.customerFeedbackReviewBool = response.data.sentiment.document.label;
                $scope.customerEmotionAnger = response.data.emotion.document.emotion.anger;
                $scope.customerEmotionDisgust = response.data.emotion.document.emotion.disgust;
                $scope.customerEmotionFear = response.data.emotion.document.emotion.fear;
                $scope.customerEmotionJoy = response.data.emotion.document.emotion.joy;
                $scope.customerEmotionAnger = response.data.emotion.document.emotion.sadness;
            },
                function (data, status, headers, config) {
                    alert(CONFIG.get('API_FAIL_TEXT'));
                });
            

        }

        //Create Hard Coded Model

        $scope.CustomerData =
            {
            headerRow: ['CreditTerms', 'On Time Shipping', 'Total Revenue', 'Gross Profit'],
            Rows: [
                { Row: [60, 100, 300000023, 34423062484] },
                { Row: [61, 98, 23446713, 35628142] },
                { Row: [64, 99, 2537500263, 23768494] },
                { Row: [67, 89, 3236823128, 24356194] }]
            
            }
        var lineChart;
        function createGraphImage()
        {
            var labelPoints = [];
            var dataPoints = [];
            var test;
            if ($scope.year == '')
                $scope.year = '2015';

            angular.forEach($scope.monthWiseData, function (value, key) {
                labelPoints.push(MONTH.get(key) + parseInt(($scope.year),10));
                dataPoints.push(value);
                console.log("value is:" + value + "Key is :" + key);
            });
            $scope.year = '';

            console.log(labelPoints);
            if (lineChart)
                lineChart.destroy();
            console.log(dataPoints);
            var ctx = document.getElementById("myAreaChart1");
            lineChart = new Chart(ctx, {
                type: 'line',
                data: {
                    //labels: ["Jan" + $scope.year, "Feb" + $scope.year, "Mar" + $scope.year, "April" + $scope.year, "May" + $scope.year, "June" + $scope.year, "July" + $scope.year, "Aug" + $scope.year, "Sept" + $scope.year, "Oct" + $scope.year, "Nov" + $scope.year, "Dec" + $scope.year],
                    labels: labelPoints,
                    datasets: [{
                        label: "Sessions",
                        lineTension: 0.3,
                        backgroundColor: "rgba(2,117,216,0.2)",
                        borderColor: "rgba(2,117,216,1)",
                        pointRadius: 5,
                        pointBackgroundColor: "rgba(2,117,216,1)",
                        pointBorderColor: "rgba(255,255,255,0.8)",
                        pointHoverRadius: 5,
                        pointHoverBackgroundColor: "rgba(2,117,216,1)",
                        pointHitRadius: 20,
                        pointBorderWidth: 2,
                        data: dataPoints,
                    }],
                },
                options: {
                    scales: {
                        xAxes: [{
                            time: {
                                unit: 'date'
                            },
                            gridLines: {
                                display: false
                            },
                            ticks: {
                                maxTicksLimit: 30
                            }
                        }],
                        yAxes: [{
                            ticks: {
                                min: 0,
                                max: 1,
                                maxTicksLimit: 5
                            },
                            gridLines: {
                                color: "rgba(0, 0, 0, .125)",
                            }
                        }],
                    },
                    legend: {
                        display: false
                    }
                }
            });
          
        }
        var myLineChart;
        function createStateWiseChurnGraph(stateWiseChurnRate)
        {
            var labelPoints = [];
            var dataPoints = [];
            
            angular.forEach(stateWiseChurnRate, function (value, key) {
                labelPoints.push(COUNTRY.get(key));
                dataPoints.push(value);
                console.log("value is:" + value + "Key is :" + key);
            });

           //$("#myBarChart1").html('');
            if (myLineChart)
                myLineChart.destroy();

            var ctx = document.getElementById("myBarChart1");
            myLineChart = new Chart(ctx, {
                type: 'horizontalBar',
                data: {
                    labels: labelPoints,
                    datasets: [{
                        label: "Revenue",
                        backgroundColor: "rgba(2,117,216,1)",
                        borderColor: "rgba(2,117,216,1)",
                        data: dataPoints,
                    }],
                },
                options: {
                    scales: {
                        xAxes: [{
                            time: {
                                unit: 'month'
                            },
                            gridLines: {
                                display: true
                            },
                            ticks: {
                                //maxTicksLimit: 6
                                min: 0,
                                max: 1,
                                maxTicksLimit: 5
                            }
                        }],
                        yAxes: [{
                            ticks: {
                                //min: 0,
                                //max: 1,
                                //maxTicksLimit: 5
                                maxTicksLimit: 6
                            },
                            gridLines: {
                                display: false
                            }
                        }],
                    },
                    legend: {
                        display: false
                    }
                }
            });
        }
        


          

    }

    myApp.controller("wpfController", myController);

    //angular.module('myModule').controller("renderModelController", ['$scope', function ($scope)
    var RenderModelController = function ($scope)
    {
        $scope.showClearConfirmationPopup = false;
        $scope.Coversheet =
            {
                engagement: { number: '8456712', name: 'Project11 Callisto Cap Markets Opinion 8.15' },
                clients: { number: '55513', name: 'VMI Holdings Corp.' },
                subjects: { number: '65436', name: 'Vision CapitalLtd' },
                valuationDate: [{ date: '1/1/2017' }, { date: '1/1/2012' }, { date: '1/4/2001' }],
                notes: [{ enteredBy: 'Analyst', enteredOn: '05/06/2017', text: 'All the required Files are Submitted' }],
                documents: [
                    { document: { id: 'A1001', displayName: 'PIF Form', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'true' }, note: '' },
                    { document: { id: 'A1002', displayName: 'PIF Form1', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'false' }, note: '' },
                    { document: { id: 'A1003', displayName: 'PIF Form2', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'false' }, note: '' },
                    { document: { id: 'A1004', displayName: 'PIF Form3', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'false' }, note: '' },
                    { document: { id: 'A1005', displayName: 'PIF Form4', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'true' }, note: '' },
                    { document: { id: 'A1006', displayName: 'PIF Form5', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'false' }, note: '3333' },
                    { document: { id: 'A1007', displayName: 'PIF Form6', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'true' }, note: '' },
                    { document: { id: 'A1008', displayName: 'PIF Form7', required: 'true', electronicAllowed: 'true', status: '', category: 'Engagement Documents', categoryId: 1 }, submissionMode: { hardCopy: 'true', electronic: 'true' }, note: '' },
                    { document: { id: 'T1001', displayName: 'Transaction Information Sheet', required: 'False', electronicAllowed: 'False', status: '', category: 'Transaction Documents', categoryId: 2 }, submissionMode: { hardCopy: 'False', electronic: 'False' }, note: '' },
                    { document: { id: 'T1004', displayName: 'Transaction Information Sheet2', required: 'False', electronicAllowed: 'False', status: '', category: 'Transaction Documents', categoryId: 2 }, submissionMode: { hardCopy: 'False', electronic: 'False' }, note: '' },
                { document: { id: 'S1001', displayName: 'Supporting and Other Information Sheet', required: 'False', electronicAllowed: 'False', status: '', category: 'Supporting and other Documents', categoryId: 3 }, submissionMode: { hardCopy: 'False', electronic: 'False' }, note: '' }
                ]
            };
        $scope.toggleModal = function () {
            $scope.showClearConfirmationPopup = !$scope.showClearConfirmationPopup;
        };
        $scope.closeModal = function () {
            $('#clearAllDocsWarning').modal('hide');
        }
        $scope.confirmClear = function () {
            if ($scope.selectionMade())
                $scope.updateModelStatusOnClick(selectedFiles, CONFIG.get('CLEAR_ACTION'))
            //Method to update the values
            angular.forEach($scope.Coversheet.documents, function (value, key) {
                $scope.Coversheet.documents[key].note = '';
            });
            $scope.removeIconClasses(null, null, true);
            $('#clearAllDocsWarning').modal('hide');
            $scope.enableAndDisableAddClearBtn();
        }
        $scope.selectionMade = function ()
        {
            if(angular.element(document.querySelectorAll('.wpfDocuments')).length>0)
                return true
            else return false
        }

        $scope.updateModelStatusOnClick = function (selectedFiles,action) {
            angular.forEach(selectedFiles, function (value, key) {
                var docId = '', status = '';
                docId = value.getAttribute('docid');
                action == 'clear' ? status = '' : status = 'submitted';
                var documentToAdd = angular.element(value).find('.availableDocument');
                var unavailableDocuments = angular.element(value).find('.unAvailableDocument');
                if (documentToAdd.length>0) {
                    angular.forEach($scope.Coversheet.documents, function (value, key) {
                        if ($scope.Coversheet.documents[key].document.id == docId)

                            $scope.Coversheet.documents[key].document.status = status;
                    });
                } else if (unavailableDocuments.length > 0) {
                    angular.forEach($scope.Coversheet.documents, function (value, key) {
                        if ($scope.Coversheet.documents[key].document.id == docId)

                            $scope.Coversheet.documents[key].document.status = 'unAvailable';
                    });

                }
                else
                {
                    angular.forEach($scope.Coversheet.documents, function (value, key) {
                        if ($scope.Coversheet.documents[key].document.id == docId)

                            $scope.Coversheet.documents[key].document.status = '';
                    });
                }
               
             

            });

        };

        
        /****************************************************Events Clicks******************************************************/
      
        //Html Event Binding
        $scope.handleDocumentclicks = function (event) {
            var currentElement = '', classToAdd = '', currentDocumentId = '', fileImageId = '', currentElementIcon = '', parentDiv = '';
            currentElement = angular.element(event.currentTarget);
            classToAdd = currentElement.attr('data-status');
            currentDocumentId = currentElement.attr('data-documentid');
            fileImageId = angular.element(document.querySelector("#fileImg_" + currentDocumentId));
            currentElementIcon = angular.element(document.querySelector("#" + currentElement.attr('id')));
            parentDiv = angular.element(document.querySelector('#doc_' + currentDocumentId));
            if (!currentElementIcon.hasClass(classToAdd)) {
                $scope.removeIconClasses(fileImageId, currentDocumentId);
                parentDiv.addClass('wpfDocuments');
                if (classToAdd == DOC_STATUS.get('UPLOAD_STATUS'))
                    angular.element(document.querySelector("#availableIcon_" + currentDocumentId)).addClass('availableDocument')
                fileImageId.addClass(classToAdd);
                currentElementIcon.addClass(classToAdd);
                //Add Variable to the parent Class 
            }
            else
                $scope.removeIconClasses(fileImageId, currentDocumentId);
            //Verify that if Add or clear needs to Enable
            $scope.enableAndDisableAddClearBtn();
            $scope.updateModelStatusOnClick(parentDiv);
        }
        //HtmlRemove Class for document Icons
        $scope.removeIconClasses = function (fileImageId, currentDocumentId, clearAll) {

            if (clearAll) {
                angular.element(document.querySelectorAll('.selectionIcons')).removeClass('availableDocument unAvailableDocument uploadDocument');
                angular.element(document.querySelectorAll('.imageIcon')).removeClass('availableDocument unAvailableDocument uploadDocument');
                angular.element(document.querySelector('.wpfDocuments')).removeClass('wpfDocuments');
                return;
            }
            fileImageId.removeClass('unAvailableDocument uploadDocument availableDocument');
            angular.element(document.querySelectorAll('.docIcons_' + currentDocumentId)).removeClass('availableDocument unAvailableDocument uploadDocument');
            angular.element(document.querySelector('#doc_' + currentDocumentId)).removeClass('wpfDocuments');
        }

        //Handle Clear and Add button Links
        $scope.CoversheetClearBtn = function ()
        {
            //Check any selections have been made or not
            var selectedFiles = '';
            selectedFiles = angular.element(document.querySelectorAll('.wpfDocuments'));
            if (selectedFiles.length > 0)
                $scope.toggleModal();

        }
        $scope.CoversheetAddbtn = function ()
        {
            var selectedFiles = '';
            selectedFiles = angular.element(document.querySelectorAll('.wpfDocuments'))
            if (selectedFiles.length > 0)
            {
                  $scope.updateModelStatusOnClick(selectedFiles);
                

            }
                //Method to add documents in the select area
                

                //angular.forEach(selectedFiles, function (value,key) {
                //    var docId = value.getAttribute('docid');

                //        angular.forEach($scope.Coversheet.documents, function (value, key) {
                //            if($scope.Coversheet.documents[key].document.id == docId)
                //                $scope.Coversheet.documents[key].document.status = 'submitted'
                //        });

                //});
            //Method to verify if the document is already in the select document section
        }

        //Method to update the status of the documents when gets cleared

    };
    
    myApp.directive('modal', function () {
        return {
            template: '<div class="modal fade">' +
                '<div class="modal-dialog">' +
                  '<div class="modal-content">' +
                    '<div class="modal-header">' +
                      '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' +
                      '<h4 class="modal-title">{{ title }}</h4>' +
                    '</div>' +
                    '<div class="modal-body" ng-transclude></div>' +
                  '</div>' +
                '</div>' +
              '</div>',
            restrict: 'E',
            transclude: true,
            replace: true,
            scope: true,
            link: function postLink(scope, element, attrs) {
                scope.title = attrs.title;

                scope.$watch(attrs.visible, function (value) {
                    if (value == true)
                        $(element).modal('show');
                    else
                        $(element).modal('hide');
                });

                $(element).on('shown.bs.modal', function () {
                    scope.$apply(function () {
                        scope.$parent[attrs.visible] = true;
                    });
                });

                $(element).on('hidden.bs.modal', function () {
                    scope.$apply(function () {
                        scope.$parent[attrs.visible] = false;
                    });
                });
            }
        };
    });
    myApp.filter('electronicSubmissionFilter', function () {
        return function (collection,keyname) {
            var test = '';
            var output = [];

            angular.forEach(collection, function (value,key) {
                if (value.electronic)
                {
                    output.push(value.electronic);
                }
            });
            return output;
        }
    });

    myApp.controller('renderModelController', RenderModelController);

    myApp.filter('uniqueTabs', function () {
        return function (collection, keyname) {
            var output = [],
                keys = [];
            //return
            angular.forEach(collection, function (item) {
                var key = item.document[keyname];
                if (keys.indexOf(key) === -1) {
                    keys.push(key);
                    output.push(item);
                }
            });

            return output;
        };
    });
    RenderModelController.$inject = ['$scope'];



    

})();