function commit(commitDoc) {
    var context = getContext();
    var collection = context.getCollection();
    var collectionLink = collection.getSelfLink();
    var errorCodes = {
        BAD_REQUEST: 400,
        NOT_FOUND: 404,
        CONFLICT: 409,
        NOT_ACCEPTED: 499
    };
    var eventSourceId = commitDoc.eventsource_id;
    var artifact = commitDoc.event_source_artifact;
    var currentCommit = commitDoc.commit;
    
    var concurrencyQuery = {
        query: "SELECT TOP 1 c.commit FROM Commits c WHERE c.eventsource_id = @eventsource_id and c.event_source_artifact = @event_source_artifact and c.partitionKey = @partitionKey ORDER BY c.commit DESC",
        parameters: [{ name: "@eventsource_id", value: eventSourceId }, { name: "@event_source_artifact", value: artifact }, { name: "@partitionKey", value: commitDoc.partitionKey }]
    };

    var accepted = collection.queryDocuments(collectionLink, concurrencyQuery, context, onConcurrencyCheck);
    if (!accepted) throw new Error("Concurrency query not accepted.");
    
    function onConcurrencyCheck(err, doc, options) {
        if (err) throw err;
    
        var lastCommit = doc.length > 0 ? doc[0].commit : 0;
        if (lastCommit == currentCommit) {
            //console.log(`latest commit in eventstore ${lastCommit} - tried to commit ${currentCommit}`);
            throw new DuplicateConcurrencyError("Concurrency conflict. Tried to resubmit" + lastCommit + ")");
        }
        if(currentCommit < lastCommit) {
            //console.log(`latest commit in eventstore ${lastCommit} - tried to commit ${currentCommit}`);
            throw new StaleVersionConcurrencyError("Stale Version. Expected version greater than" + lastCommit + ", tried to commit version " + currentCommit + ")");
        }
    
        generateCommitSequenceNumber(commitDoc,collection,collectionLink, createCommit);
    }
    
    function createCommit(doc) {
        var accepted = collection.createDocument(collectionLink,
            doc,
            function (err, documentCreated) {
                if (err) {
                    throw new Error('Error' + err.message);
                }   
                context.getResponse().setBody(documentCreated._id);
            });
        if (!accepted) return;
    }

    function generateCommitSequenceNumber(insertedDoc, collection, collectionLink, callback) {  
        setAndIncrementCommitSequenceNumber(insertedDoc, callback);
    }
        
    function setAndIncrementCommitSequenceNumber(commit, callback) {
        var isAccepted = collection.queryDocuments(collectionLink, 'SELECT * FROM commits c WHERE c.isMetadata = true', function (err, feed, options) {
            throwIfError(err);
            var sequenceDoc = getSequenceDoc(feed,commit.partitionKey);
            setCommitNumber(commit,sequenceDoc.doc);
            persistSequenceNumber(sequenceDoc);
            callback(commit);
        });
        checkAccepted(isAccepted,!errorCodes.NOT_ACCEPTED, "Retrieving the sequence number for the commit failed");
    }
    
    function getSequenceDoc(feed, partitionKey){
        var sequenceDoc = { "doc": null, "isFirstSequence": !feed || !feed.length };
        if (sequenceDoc.isFirstSequence) {
            sequenceDoc.doc = createNewMetadataDoc(partitionKey);
        }
        else {
            sequenceDoc.doc = feed[0];
        }
        return sequenceDoc;
    }
    
    function persistSequenceNumber(sequence){
        var isAccepted;
        if(sequence.isFirstSequence){
            isAccepted = collection.createDocument(collectionLink, sequence.doc, context, function (err, feed, options) {
                throwIfError(err);
                // Note: in case concurrent updates causes conflict with ErrorCode.RETRY_WITH, we can't read the meta again
                //       and update again because due to Snapshot isolation we will read same exact version (we are in same transaction).
                //       We have to take care of that on the client side.
            });
        } else {
            isAccepted = collection.replaceDocument(sequence.doc._self, sequence.doc, function (err, feed, options) {
                throwIfError(err);
                // Note: in case concurrent updates causes conflict with ErrorCode.RETRY_WITH, we can't read the meta again
                //       and update again because due to Snapshot isolation we will read same exact version (we are in same transaction).
                //       We have to take care of that on the client side.

            });
        }
        checkAccepted(isAccepted,!errorCodes.NOT_ACCEPTED, "Updating the sequence number for the commit failed");
    }
    
    function createNewMetadataDoc(partitionKey){
        return {
            "id": "metadata",
            "isMetadata": true,
            "sequence": 1,
            "partitionKey": partitionKey
        };
    }
    
    function setCommitNumber(commitDoc, sequenceDoc){
        commitDoc.id = sequenceDoc.sequence.toString();
        commitDoc._id = sequenceDoc.sequence;
        sequenceDoc.sequence = sequenceDoc.sequence+1;
    }

    function checkAccepted(isAccepted, code, message) {
        if(isAccepted)
            return;
        var code = code || errorCodes.NOT_ACCEPTED;
        var msg = message || "The request was not accepted. You can retry from the Client";
        throw new Error(code,msg);
    }
    
    function throwIfError(err)
    {
        if(err) 
            throw err;
    }

    class DuplicateConcurrencyError extends Error {
        constructor(message,cause){
            super(message);
            this.cause = cause;
            this.name = "Duplicate";
        }
    }

    class StaleVersionConcurrencyError extends Error {
        constructor(message,cause){
            super(message);
            this.cause = cause;
            this.name = "Stale";
        }
    }    
}