var Logger, SisoDbClient;

Logger = (function() {

  function Logger() {
    this.enabled = false;
  }

  Logger.prototype.on = function() {
    return this.enabled = true;
  };

  Logger.prototype.off = function() {
    return this.enabled = false;
  };

  Logger.prototype.write = function() {
    if (this.enabled === false) return;
    if (typeof arguments[0] !== 'function') {
      if (typeof console !== "undefined" && console !== null) {
        console.log(arguments[0]);
      }
    }
    if (typeof arguments[0] === 'function') {
      return typeof console !== "undefined" && console !== null ? console.log(arguments[0]()) : void 0;
    }
  };

  return Logger;

})();

SisoDbClient = (function() {

  function SisoDbClient(url) {
    this.url = url;
    this._ws = jXSockets.xWebSocket(this.url, jXSockets.WEBSOCKET);
    this.logger = new Logger;
    this._trigger = function(event, doc) {
      this.logger.write("before " + event);
      if (doc) this.logger.write("with doc: " + (JSON.stringify(doc)));
      this._ws.trigger(event, doc);
      return this.logger.write("after " + event);
    };
    this._bind = function(event, handler) {
      this.logger.write("binding " + event);
      this._ws.bind(event, handler);
      return this.logger.write('binding complete');
    };
  }

  SisoDbClient.prototype.connect = function() {
    this.logger.write('connecting');
    this._ws.bind('open', function() {});
    return this.logger.write('connected');
  };

  SisoDbClient.prototype.ping = function(msg) {
    return this._trigger('Ping', {
      Message: msg
    });
  };

  SisoDbClient.prototype.onPinged = function(handler) {
    return this._bind('OnPinged', handler);
  };

  SisoDbClient.prototype.insert = function(structureName, structure) {
    return this._trigger('Insert', {
      StructureName: structureName,
      Json: JSON.stringify(structure)
    });
  };

  SisoDbClient.prototype.onInserted = function(handler) {
    return this._bind('OnInserted', handler);
  };

  SisoDbClient.prototype.deleteById = function(structureName, id) {
    return this._trigger('DeleteById', {
      StructureName: structureName,
      Id: id
    });
  };

  SisoDbClient.prototype.onDeletedById = function(handler) {
    return this._bind('OnDeletedById', handler);
  };

  SisoDbClient.prototype.getById = function(structureName, id) {
    return this._trigger('GetById', {
      StructureName: structureName,
      Id: id
    });
  };

  SisoDbClient.prototype.onGetById = function(handler) {
    return this._bind('OnGetById', handler);
  };

  SisoDbClient.prototype.query = function(structureName, predicate) {
    return this._trigger('Query', {
      StructureName: structureName,
      Predicate: predicate
    });
  };

  SisoDbClient.prototype.onQuery = function(handler) {
    return this._bind('OnQuery', handler);
  };

  return SisoDbClient;

})();
