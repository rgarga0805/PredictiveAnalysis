/**
 * Working Paper File
 * 
 * @package proshpere
 * @subpackage core
 */

/**
 * configuration.js defines configure parameters commonly used in the application
 * 
 */

// Define variable to check if environment is set to DEBUG or not
var LOCAL = 0;
var PORT_NUMBER = '';
var BASE_URL = "https://prosphere-secure.teamworkathletic.com";
//var PS2_SERVER_2 = 0;
/**
 * Returns the Base URL
 * 
 * @return string - Base URL
 */
getBaseUrl = function () {
    var baseUrlClient = location.protocol + "//" + location.hostname
            + (location.port && ":" + location.port) + "/";
    return baseUrlClient + '';
};

/**
 * Defines the configuration parameter
 * 
 */
var CONFIG = (function () {
    var CONSTANT = {
        GOOGLE_ANALYTICS_PROPERTY_ID: "UA-9262906-5", // twa google analytics property Id,
        USE_GA: true,
        DEBUG: false, // Debug mode
        DEBUG_STRING: '=== DEBUG MODE === DEBUG MODE === DEBUG MODE ===\n', // Debug
        //GET_ANALYTICS_URL: 'http://localhost:51390/api/Analytics/GetAnnualChurnRate?customerName=Ford%20Motor',
        GET_ANALYTICS_URL:' http://localhost:51390/api/Analytics/GetChurnRate',
        GET_NPL_URL: 'http://localhost:51390/api/Analytics/NPLResults',
        GET_CUSTOMER_GROSS_URL: 'http://localhost:51390/api/Analytics/NPLResults', 
        GET_CUSTOMER_DETAILS: 'http://localhost:51390/api/Analytics/GetCustomerInfo',
        GET_KPI_LIST:'http://localhost:51390/api/Analytics/GetKPIs',
        CLEAR_ACTION: 'clear',
        UPLOAD_ICON: 5,
        BULLETS_PAGINATION_MIN_WIDTH: 410,
        MIN_BULLET_IMAGES: 5,
        DEFAULT_EMAIL_UNIFORM_TYPE: -1,
        ROSTER_LIVE_CHAT_ANCHOR_ID: "liLiveChatRoster",
        UNIFORM_LIVE_CHAT_ANCHOR_ID: "liLiveChat",
        GRAPHICS_SINK_COMMAND_FORMAT: "jpg"
    };
    return {
        get: function (key, defaultValue) {
            return CONSTANT[key] !== undefined ? CONSTANT[key] : defaultValue;
        }
    };
}());

var CUSTOMER_IMAGE = (function () {
    var IMAGE = {
        '1001': 'fordmotor.png',
        '1004': 'pepsico.png',
        '1005': 'p&g.jpg',
        '1002': 'unilever.png',
        '1003': 'hp.png'
    };
    return {
        get: function (key) {
            return IMAGE[key];
        }
    };
}());


var COUNTRY = (function () {
    var NAME = {
        '1001': 'USA',
        '1002': 'Brazil',
        '1003': 'China',
        '1004': 'Spain',
        '1005': 'UK'
    };
    return {
        get: function (key) {
            return NAME[key];
        }
    };
}());

var MONTH = (function () {
    var NAME = {
        '1': 'Jan',
        '2': 'Feb',
        '3': 'March',
        '4': 'Apr',
        '5': 'May',
        '6': 'Jun',
        '7': 'Jul',
        '8': 'Aug',
        '9': 'Sep',
        '10': 'Oct',
        '11': 'Nov',
        '12': 'Dec'
    };
    return {
        get: function (key) {
            return NAME[key];
        }
    };
}());
var CustomerIDPerKPI = (function () {
    var NAME = {
        '1001': 0,
        '1002': 1,
        '1003': 2,
        '1004': 3,
        '1005':4
    };
    return {
        get: function (key) {
            return NAME[key];
        }
    };
}());





/**
 * Football popup sleeves images for sports catagories
 * 
 */
var NOTIFICATION_SLEEVES_IMAGE = (function () {
    var IMAGES = {
        "FSS": "frontQB2.png",
        "WSS": "frontQB.png"
    };
    return {
        get: function (key) {
            return IMAGES[key.toUpperCase()];
        }
    };
}());



