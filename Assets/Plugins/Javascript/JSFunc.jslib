mergeInto(LibraryManager.library, {

    GetSubID: function () {
            var returnStr = window.subID;
            var bufferSize = lengthBytesUTF8(returnStr) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(returnStr, buffer, bufferSize);
            return buffer;
        },

    SetScore: function (score) {
            window.score = score ;
        },
    Alert: function (str) {
            window.alert(str);
        },
    SetEnd: function() {
        window.endGame();
    },

    SetEndTrainingRL: function() {
        window.endTrainingRL();
    },

    SetEndTrainingPerceptual: function() {
        window.endTrainingPerceptual();
    },
    
    
});
