(function () {
    'use strict';

    angular
        .module('simoonaApp.Vacations')
        .controller('vacationsController', vacationsController);

    vacationsController.$inject = [
        '$rootScope',
        '$scope',
        'vacationsRepository',
        'notifySrv',
        'shroomsFileUploader',
        'authService',
        'errorHandler'
    ];

    function vacationsController($rootScope, $scope, vacationsRepository, notifySrv,
        shroomsFileUploader, authService, errorHandler) {
        var vm = this;

        var uploadConfig = {
            allowed: ["xls", "XLS"],
            sizeLimit: 5000000,
        };
        var xlsVacationFile = null;

        $rootScope.pageTitle = 'common.vacations';

        //vm.vacationList = [];
        vm.availableDays = {};
        vm.isLoading = false;

        vm.uploadVacationFile = uploadVacationFile;
        $scope.fileAttached = fileAttached;

        init();

        //////////

        function init() {
            // vacationsRepository.getVacationHistory().then(function (response) {
            //     vm.vacationList = response;
            //     vm.isLoading = false;
            // }, function(error) {
            //     vm.isLoading = false;
            //     errorHandler.handleErrorMessage(error);
            // });

            getAvailableDays();
        }

        function fileAttached(input) {
            var isValid;

            if (input.value) {
                isValid = shroomsFileUploader.validateCustomFileType(
                    input.files,
                    uploadConfig,
                    showUploadAlert
                );
            }

            if (isValid) {
                xlsVacationFile = shroomsFileUploader.fileListToArray(input.files);
                uploadVacationFile();
            }

            $scope.$apply();
        }

        function uploadVacationFile() {
            vacationsRepository.uploadVacationFile(xlsVacationFile).then(function (response) {
                    notifySrv.success('vacations.vacationTimeReportImportedSuccessfully');

                    getAvailableDays();
                }, errorHandler.handleErrorMessage);
        }

        function showUploadAlert(status) {
            var statuses = {
                success: success,
                sizeError: sizeError,
                typeError: typeError
            }

            if (statuses.hasOwnProperty(status)) {
                statuses[status]();
            }

            function success() {}

            function sizeError() {
                notifySrv.error('vacations.fileTooLarge');
            }

            function typeError() {
                notifySrv.error('vacations.fileTypeError');
            }
        }

        function getAvailableDays() {
            vacationsRepository.getAvailableDays().then(function (response) {
                vm.availableDays = response;
            }, errorHandler.handleErrorMessage);
        }
    }
})();
