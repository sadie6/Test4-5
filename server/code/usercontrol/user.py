# -*- coding: utf-8 -*-
import sys
sys.path.append('..')
import pubglobalmanager

class User():
	def __init__(self,username,password):
		self.username = username
		self.password = password

	def CheckLogin(self):
		_sql = "select password from user where username='%s'" % self.username
		res = pubglobalmanager.CallManagerFunc('dbctrl', 'Query', _sql)
		if res:									#判断用户名是否存在，不存在则新建一个
			if(res[0][0] == self.password):        #判断密码是否正确
				return True
			else:
				return False
		else:
			return self.AddUser()

	def AddUser(self):
		_sql = "insert into user values('%s', '%s')" % (self.username, self.password)
		res = pubglobalmanager.CallManagerFunc('dbctrl', 'ExecSql', _sql)
		if res == 0:
			return True
		else:
			return False



User('liusa','sdadsa')