class Logger
  constructor: ->
    @enabled = false
  on: ->
    @enabled = true
  off: ->
    @enabled = false
  write: ->
    return if @enabled is false
    console?.log arguments[0] if typeof arguments[0] isnt 'function'
    console?.log arguments[0]() if typeof arguments[0] is 'function'

class SisoDbClient
  constructor: (@url) ->
    @_ws = jXSockets.xWebSocket @url, jXSockets.WEBSOCKET
    @logger = new Logger
    @_trigger = (event, doc) ->
      @logger.write "before #{event}"
      @logger.write "with doc: #{JSON.stringify doc}" if doc
      @_ws.trigger event, doc
      @logger.write "after #{event}"
    @_bind = (event, handler) ->
      @logger.write "binding #{event}"
      @_ws.bind event, handler
      @logger.write 'binding complete'
  connect: ->
    @logger.write 'connecting'
    @_ws.bind 'open', ->
    @logger.write 'connected'
  ping: (msg) ->
    @_trigger 'Ping', Message: msg
  onPinged: (handler) ->
    @_bind 'OnPinged', handler
  insert: (structureName, structure) ->
    @_trigger 'Insert', StructureName: structureName, Json: JSON.stringify structure
  onInserted: (handler) ->
    @_bind 'OnInserted', handler
  deleteById: (structureName, id) ->
    @_trigger 'DeleteById', StructureName: structureName, Id: id
  onDeletedById: (handler) ->
    @_bind 'OnDeletedById', handler
  getById: (structureName, id) ->
    @_trigger 'GetById', StructureName: structureName, Id: id
  onGetById: (handler) ->
    @_bind 'OnGetById', handler
  query: (structureName, predicate) ->
    @logger.write 'querying'
    query = {}
    predicate query
    parsedQuery = Where:[]
    for k,v of query
      parsedQuery.Where.push {Path: k, Value: v}
    @logger.write parsedQuery
    @_trigger 'Query', StructureName: structureName, Json: JSON.stringify parsedQuery
  onQuery: (handler) ->
    @_bind 'OnQuery', handler