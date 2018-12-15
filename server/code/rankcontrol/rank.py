# -*- coding: utf-8 -*-
import sys
sys.path.append('..')
import pubglobalmanager


class Rank():
	count = 10


	def Submit(self,username,score):
		_sql = "insert into rank values(DEFAULT,'%s', %s)" %(username, score)
		pubglobalmanager.CallManagerFunc('dbctrl', 'ExecSql', _sql)

	def GetRank(self):
		result = []
		rankinfo = {}
		_sql = "select * from rank order by score desc limit %s" % self.count
		res = pubglobalmanager.CallManagerFunc('dbctrl', 'Query', _sql)
		if res:
			for _res in res:
				rankinfo = {
				'username' : _res[1],
				'score' : _res[2]
				}
				result.append(rankinfo)
		return result


					




